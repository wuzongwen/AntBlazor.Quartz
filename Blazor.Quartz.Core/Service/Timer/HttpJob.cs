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

namespace Blazor.Quartz.Core.Service.Timer
{
    public class HttpJob : JobBase<LogUrlModel>, IJob
    {
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


            LogInfo.Url = requestUrl;
            LogInfo.RequestType = requestType.ToString();
            LogInfo.Parameters = requestParameters;

            HttpResponseMessage response = new HttpResponseMessage();
            var http = HttpHelper.Instance;
            switch (requestType)
            {
                case RequestTypeEnum.Get:
                    response = await http.GetAsync(requestUrl, headers);
                    break;
                case RequestTypeEnum.Post:
                    response = await http.PostAsync(requestUrl, requestParameters, headers);
                    break;
                case RequestTypeEnum.Put:
                    response = await http.PutAsync(requestUrl, requestParameters, headers);
                    break;
                case RequestTypeEnum.Delete:
                    response = await http.DeleteAsync(requestUrl, headers);
                    break;
            }
            var result = HttpUtility.HtmlEncode(await response.Content.ReadAsStringAsync());
            LogInfo.Result = $"<span class='result'>{result.MaxLeft(1000)}</span>";

            //添加执行记录
            var model = new JOB_EXECUTION_LOG();
            model.JOB_NAME = context.JobDetail.Key.Name;
            model.JOB_GROUP = context.JobDetail.Key.Group;
            model.REQUEST_URL = LogInfo.Url;
            model.REQUEST_TYPE = LogInfo.RequestType;
            model.HEADERS = headersString;
            model.REQUEST_DATA = requestParameters;
            model.RESPONSE_DATA = HttpUtility.HtmlDecode(result);
            model.BEGIN_TIME = LogInfo.BeginTime;

            if (!response.IsSuccessStatusCode)
            {
                model.EXECUTION_STATUS = ExecutionStatusEnum.Failure;
                LogInfo.ErrorMsg = $"<span class='error'>{result.MaxLeft(3000)}</span>";
                await ErrorAsync(LogInfo.JobName, new Exception(result.MaxLeft(3000)), JsonConvert.SerializeObject(LogInfo), MailLevel);
                context.JobDetail.JobDataMap[QuartzConstant.EXCEPTION] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
            }
            else
            {
                try
                {
                    //这里需要和请求方约定好返回结果约定为HttpResultModel模型
                    var httpResult = JsonConvert.DeserializeObject<HttpResultModel>(HttpUtility.HtmlDecode(result));
                    if (!httpResult.IsSuccess)
                    {
                        model.EXECUTION_STATUS = ExecutionStatusEnum.Failure;
                        LogInfo.ErrorMsg = $"<span class='error'>{httpResult.ErrorMsg}</span>";
                        await ErrorAsync(LogInfo.JobName, new Exception(httpResult.ErrorMsg), JsonConvert.SerializeObject(LogInfo), MailLevel);
                        context.JobDetail.JobDataMap[QuartzConstant.EXCEPTION] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
                    }
                    else
                        model.EXECUTION_STATUS = ExecutionStatusEnum.Success;
                    await InformationAsync(LogInfo.JobName, JsonConvert.SerializeObject(LogInfo), MailLevel);
                }
                catch (Exception)
                {
                    model.EXECUTION_STATUS = ExecutionStatusEnum.Failure;
                    await InformationAsync(LogInfo.JobName, JsonConvert.SerializeObject(LogInfo), MailLevel);
                }
            }
            //插入日志记录
            var res = await DbContext.ExecuteAsync($@"INSERT INTO {QuartzConstant.TablePrefix}JOB_EXECUTION_LOG ([JOB_NAME]
                            ,[JOB_GROUP]
                            ,[EXECUTION_STATUS]
                            ,[REQUEST_URL]
                            ,[REQUEST_TYPE]
                            ,[HEADERS]
                            ,[REQUEST_DATA]
                            ,[RESPONSE_DATA]
                            ,[BEGIN_TIME]
                            ) VALUES(@JOB_NAME,@JOB_GROUP,@EXECUTION_STATUS,@REQUEST_URL,@REQUEST_TYPE,@HEADERS,@REQUEST_DATA,@RESPONSE_DATA,@BEGIN_TIME)", model);
        }
    }
}
