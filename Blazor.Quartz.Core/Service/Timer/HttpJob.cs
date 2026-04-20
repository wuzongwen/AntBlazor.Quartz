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
using Blazor.Quartz.Common.DingTalkRobot.Robot;
using System.Text.RegularExpressions;
using System.Collections;
using Blazor.Quartz.Core.Dependency;
using System.Threading;
using Blazor.Quartz.Common;

namespace Blazor.Quartz.Core.Service.Timer
{
    public class HttpJob : JobBase<LogUrlModel>, IJob
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Constructor for DI
        public HttpJob(IHttpClientFactory httpClientFactory) : base(new LogUrlModel())
        {
            _httpClientFactory = httpClientFactory;
        }

        // Parameterless constructor kept for backward compatibility (will try ServiceLocator)
        public HttpJob() : base(new LogUrlModel())
        {
            _httpClientFactory = ServiceLocator.ServiceProvider?.GetService(typeof(IHttpClientFactory)) as IHttpClientFactory;
        }

        public override async Task NextExecute(IJobExecutionContext context)
        {
            //获取相关参数
            var requestUrl = context.JobDetail.JobDataMap.GetString(QuartzConstant.REQUESTURL)?.Trim();
            requestUrl = requestUrl?.IndexOf("http") == 0 ? requestUrl : "http://" + requestUrl;
            var requestParameters = context.JobDetail.JobDataMap.GetString(QuartzConstant.REQUESTPARAMETERS);
            var headersString = context.JobDetail.JobDataMap.GetString(QuartzConstant.HEADERS);
            var headers = headersString != null ? JsonConvert.DeserializeObject<Dictionary<string, object>>(headersString?.Trim()) : null;
            var requestType = (RequestTypeEnum)int.Parse(context.JobDetail.JobDataMap.GetString(QuartzConstant.REQUESTTYPE));
            var TimeOut = AppConfig.DefaultJobTimeout;
            if (!string.IsNullOrEmpty(context.JobDetail.JobDataMap.GetString(QuartzConstant.TIMEOUT)))
            {
                TimeOut = context.JobDetail.JobDataMap.GetIntValueFromString(QuartzConstant.TIMEOUT);
            }
            var CovenantReturnModel = false;
            if (!string.IsNullOrEmpty(context.JobDetail.JobDataMap.GetString(QuartzConstant.CovenantReturnModel)))
            {
                CovenantReturnModel = context.JobDetail.JobDataMap.GetBooleanValueFromString(QuartzConstant.CovenantReturnModel);
            }

            // populate LogInfo early so JobBase will persist values even on early return
            LogInfo.Url = requestUrl;
            LogInfo.RequestType = requestType.ToString();
            LogInfo.Parameters = requestParameters;
            LogInfo.Req_Url = requestUrl;
            LogInfo.Req_Type = requestType.ToString();
            LogInfo.Headers = headersString;
            LogInfo.Req_Data = requestParameters;
            LogInfo.TimeOut = TimeOut;

            HttpResponseMessage response = null;

            // parse request body if needed
            Dictionary<string, object> reqData = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(requestParameters))
            {
                try
                {
                    reqData = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestParameters.Trim());
                }
                catch
                {
                    // leave reqData empty and treat requestParameters as raw string
                }
            }

            var client = _httpClientFactory?.CreateClient("NoRetryLongRunning") ??
                         (ServiceLocator.ServiceProvider?.GetService(typeof(IHttpClientFactory)) as IHttpClientFactory)?.CreateClient("NoRetryLongRunning") ?? new HttpClient();

            using (var requestMessage = new HttpRequestMessage())
            {
                requestMessage.RequestUri = new Uri(requestUrl);
                switch (requestType)
                {
                    case RequestTypeEnum.Get:
                        requestMessage.Method = HttpMethod.Get;
                        break;
                    case RequestTypeEnum.Post:
                        requestMessage.Method = HttpMethod.Post;
                        break;
                    case RequestTypeEnum.Put:
                        requestMessage.Method = HttpMethod.Put;
                        break;
                    case RequestTypeEnum.Delete:
                        requestMessage.Method = HttpMethod.Delete;
                        break;
                }

                // add headers to request
                if (headers != null)
                {
                    foreach (var kv in headers)
                    {
                        try
                        {
                            requestMessage.Headers.TryAddWithoutValidation(kv.Key, kv.Value?.ToString());
                        }
                        catch { }
                    }
                }

                // add body for Post/Put
                if ((requestType == RequestTypeEnum.Post || requestType == RequestTypeEnum.Put) && !string.IsNullOrEmpty(requestParameters))
                {
                    var json = JsonConvert.SerializeObject(reqData.Count > 0 ? reqData : (object)requestParameters);
                    requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                // prepare cancellation token: link job/context token with per-request timeout if configured
                CancellationToken linkedToken = CancellationToken.None;
                CancellationTokenSource cts = null;
                try
                {
                    CancellationToken jobToken = CancellationToken.None;
                    try { jobToken = context.CancellationToken; } catch { }

                    if (TimeOut > 0)
                    {
                        cts = CancellationTokenSource.CreateLinkedTokenSource(jobToken);
                        cts.CancelAfter(TimeSpan.FromSeconds(TimeOut));
                        linkedToken = cts.Token;
                    }
                    else
                    {
                        linkedToken = jobToken; // no per-request timeout, rely on job cancellation token
                    }

                    if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out var uri))
                    {
                        await HandleException(context, new Exception($"URL格式非法: {requestUrl}"), "参数异常");
                        return;
                    }
                    try
                    {
                        response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, linkedToken);
                    }
                    catch (HttpRequestException ex)
                    {
                        // 捕获：DNS错误、网络不可达、SSL证书问题、请求类型(Method)不支持等
                        await HandleException(context, ex, "网络请求异常/请求参数不规范");
                        return;
                    }
                    catch (OperationCanceledException ex) // 包含 TaskCanceledException
                    {
                        if (cts != null && cts.IsCancellationRequested && !jobToken.IsCancellationRequested)
                        {
                            // 只有当我们的超时闹钟响了，且任务本身没被外部停止时，才是真正的超时
                            await HandleException(context, new TimeoutException($"请求超时，设定值为 {TimeOut} 秒"), "任务超时");
                        }
                        else if (jobToken.IsCancellationRequested)
                        {
                            await HandleException(context, new Exception("任务被取消/调度停止"), "任务取消");
                        }
                        else
                        {
                            await HandleException(context, ex, "请求被异常中断（HttpClient内部原因）");
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        // 捕获：序列化失败、代码逻辑错误等
                        await HandleException(context, ex, "系统运行异常");
                        return;
                    }
                }
                finally
                {
                    cts?.Dispose();
                }
            }

            // ensure we have a response
            if (response == null)
            {
                LogInfo.Status = ExecutionStatusEnum.Failure;
                LogInfo.ErrorMsg = "<span class='error'>未获取到响应</span>";
                LogInfo.Result = "未获取到响应";
                await ErrorAsync(LogInfo.JobName, new Exception("未获取到响应"), JsonConvert.SerializeObject(LogInfo));
                context.JobDetail.JobDataMap[QuartzConstant.EXCEPTION] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
                return;
            }

            var result = HttpUtility.HtmlEncode(Regex.Unescape(await response.Content.ReadAsStringAsync()));
            LogInfo.Req_Data = requestParameters;
            LogInfo.Result = HttpUtility.HtmlDecode(result);

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
                    if (CovenantReturnModel)
                    {
                        //这里需要和请求方约定好返回结果约定为HttpResultModel模型
                        var httpResult = JsonConvert.DeserializeObject<HttpResultModel>(HttpUtility.HtmlDecode(result));
                        if (!httpResult.isSuccess && httpResult.resCode != 0)
                        {
                            LogInfo.Status = ExecutionStatusEnum.Failure;
                            LogInfo.ErrorMsg = $"<span class='error'>{httpResult.resMsg}</span>";
                            await ErrorAsync(LogInfo.JobName, new Exception(httpResult.resMsg), JsonConvert.SerializeObject(LogInfo));
                            context.JobDetail.JobDataMap[QuartzConstant.EXCEPTION] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
                        }
                        else
                        {
                            LogInfo.Status = ExecutionStatusEnum.Success;
                        }
                    }
                    else
                    {
                        LogInfo.Status = ExecutionStatusEnum.Success;
                    }
                }
                catch (Exception ex)
                {
                    if (CovenantReturnModel && ex.Message.Contains("Unexpected character encountered while parsing value"))
                    {
                        var httpResult = HttpUtility.HtmlDecode(result);
                        LogInfo.Status = ExecutionStatusEnum.Failure;
                        await ErrorAsync(LogInfo.JobName, new Exception($"未按约定模型返回响应数据,当前响应数据:{httpResult}"), JsonConvert.SerializeObject(LogInfo));
                        throw new Exception($"未按约定模型返回响应数据,当前响应数据:{httpResult}");
                    }
                    LogInfo.Status = ExecutionStatusEnum.Failure;
                    await ErrorAsync(LogInfo.JobName, ex, JsonConvert.SerializeObject(LogInfo));
                    throw new Exception(ex.Message);
                }
            }
        }

        private async Task HandleException(IJobExecutionContext context, Exception ex, string category)
        {
            LogInfo.Status = ExecutionStatusEnum.Failure;
            // 记录格式：[异常分类] 具体错误信息
            LogInfo.ErrorMsg = $"<span class='error'><b>[{category}]</b>: {ex.Message.MaxLeft(3000)}</span>";
            LogInfo.Result = $"[{category}] {ex.Message}";

            // 记录详细堆栈到 Quartz 异常上下文
            await ErrorAsync(LogInfo.JobName, ex, JsonConvert.SerializeObject(LogInfo));
            context.JobDetail.JobDataMap[QuartzConstant.EXCEPTION] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
        }
    }
}
