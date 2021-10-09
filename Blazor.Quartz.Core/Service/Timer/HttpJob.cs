using Blazor.Quartz.Core.Const;
using Blazor.Quartz.Core.Dapper;
using Blazor.Quartz.Core.Service.App.Dto;
using Blazor.Quartz.Core.Service.App.Enum;
using Blazor.Quartz.Core.Service.Timer.Dto;
using Blazor.Quartz.Core.Service.Timer.Enum;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Talk.Extensions;
using Talk.Extensions.Helper;
using Flurl.Http;
using Blazor.Quartz.Common.DingTalkRobot.Robot;

namespace Blazor.Quartz.Core.Service.Timer
{
    public class HttpJob : JobBase<LogUrlModel>, IJob
    {
        private IFlurlResponse flurlResponse;

        public HttpJob() : base(new LogUrlModel())
        { }

        public override async Task NextExecute(IJobExecutionContext context)
        {
            //获取相关参数
            var requestUrl = context.JobDetail.JobDataMap.GetString(QuartzConstant.REQUESTURL)?.Trim();
            requestUrl = requestUrl?.IndexOf("http") == 0 ? requestUrl : "http://" + requestUrl;
            var requestParameters = context.JobDetail.JobDataMap.GetString(QuartzConstant.REQUESTPARAMETERS);
            var headersString = context.JobDetail.JobDataMap.GetString(QuartzConstant.HEADERS);
            var headers = headersString != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(headersString?.Trim()) : null;
            var requestType = (RequestTypeEnum)int.Parse(context.JobDetail.JobDataMap.GetString(QuartzConstant.REQUESTTYPE));
            var timeOut = context.JobDetail.JobDataMap.GetIntValueFromString(QuartzConstant.TIMEOUT);
            if (timeOut == 0) 
            {
                timeOut = 30;
            }

            LogInfo.Url = requestUrl;
            LogInfo.RequestType = requestType.ToString();
            LogInfo.Parameters = requestParameters;

            HttpResponseMessage response = new HttpResponseMessage();
            var http = HttpHelper.Instance;
            LogInfo.Req_Url = requestUrl;
            LogInfo.Req_Type = LogInfo.RequestType;
            LogInfo.Headers = headersString;
            LogInfo.Result = requestParameters;
            switch (requestType)
            {
                case RequestTypeEnum.Get:
                    //response = await http.GetAsync(requestUrl, headers);
                    if (headers != null) 
                    {
                        flurlResponse = await requestUrl.WithHeaders(headers).WithTimeout(timeOut).GetAsync();
                    }
                    else 
                    {
                        flurlResponse = await requestUrl.WithTimeout(timeOut).GetAsync();
                    }
                    response = flurlResponse.ResponseMessage;
                    break;
                case RequestTypeEnum.Post:
                    //response = await http.PostAsync(requestUrl, requestParameters, headers);
                    if (headers != null)
                    {
                        if (requestParameters != null)
                        {
                            flurlResponse = await requestUrl.WithHeaders(headers).WithTimeout(timeOut).PostStringAsync(requestParameters);
                        }
                        else
                        {
                            flurlResponse = await requestUrl.WithHeaders(headers).WithTimeout(timeOut).PostAsync();
                        }
                    }
                    else 
                    {
                        if (requestParameters != null)
                        {
                            flurlResponse = await requestUrl.WithTimeout(timeOut).PostStringAsync(requestParameters);
                        }
                        else 
                        {
                            flurlResponse = await requestUrl.WithTimeout(timeOut).PostAsync();
                        }
                    }
                    response = flurlResponse.ResponseMessage;
                    break;
                case RequestTypeEnum.Put:
                    //response = await http.PutAsync(requestUrl, requestParameters, headers);
                    if (headers != null)
                    {
                        if (requestParameters != null)
                        {
                            flurlResponse = await requestUrl.WithHeaders(headers).WithTimeout(timeOut).PutStringAsync(requestParameters);
                        }
                        else 
                        {
                            flurlResponse = await requestUrl.WithHeaders(headers).WithTimeout(timeOut).PutAsync();
                        }
                    }
                    else
                    {
                        if (requestParameters != null)
                        {
                            flurlResponse = await requestUrl.WithTimeout(timeOut).PutStringAsync(requestParameters);
                        }
                        else
                        {
                            flurlResponse = await requestUrl.WithTimeout(timeOut).PutAsync();
                        }
                    }
                    response = flurlResponse.ResponseMessage;
                    break;
                case RequestTypeEnum.Delete:
                    //response = await http.DeleteAsync(requestUrl, headers);
                    if (headers != null)
                    {
                        flurlResponse = await requestUrl.WithHeaders(headers).WithTimeout(timeOut).DeleteAsync();
                    }
                    else
                    {
                        flurlResponse = await requestUrl.WithTimeout(timeOut).DeleteAsync();
                    }
                    response = flurlResponse.ResponseMessage;
                    break;
            }
            var result = HttpUtility.HtmlEncode(await response.Content.ReadAsStringAsync());
            LogInfo.Result = $"<span class='result'>{result.MaxLeft(1000)}</span>";

            LogInfo.Res_Data = HttpUtility.HtmlDecode(result);

            if (!response.IsSuccessStatusCode)
            {
                LogInfo.Status = ExecutionStatusEnum.Failure;
                LogInfo.ErrorMsg = $"<span class='error'>{result.MaxLeft(3000)}</span>";
                await ErrorAsync(LogInfo.JobName, new Exception(result.MaxLeft(3000)), JsonConvert.SerializeObject(LogInfo));
                context.JobDetail.JobDataMap[QuartzConstant.EXCEPTION] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
            }
            else
            {
                try
                {
                    ////这里需要和请求方约定好返回结果约定为HttpResultModel模型
                    //var httpResult = JsonConvert.DeserializeObject<HttpResultModel>(HttpUtility.HtmlDecode(result));
                    //if (!httpResult.IsSuccess)
                    //{
                    //    LogInfo.Status = ExecutionStatusEnum.Failure;
                    //    LogInfo.ErrorMsg = $"<span class='error'>{httpResult.ErrorMsg}</span>";
                    //    await ErrorAsync(LogInfo.JobName, new Exception(httpResult.ErrorMsg), JsonConvert.SerializeObject(LogInfo));
                    //    context.JobDetail.JobDataMap[QuartzConstant.EXCEPTION] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
                    //}
                    //else
                    //    LogInfo.Status = ExecutionStatusEnum.Success;
                    LogInfo.Status = ExecutionStatusEnum.Success;
                }
                catch (Exception ex)
                {
                    LogInfo.Status = ExecutionStatusEnum.Failure;
                    await ErrorAsync(LogInfo.JobName, ex, JsonConvert.SerializeObject(LogInfo));
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
