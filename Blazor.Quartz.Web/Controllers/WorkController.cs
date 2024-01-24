using Blazor.Quartz.Common.DingTalkRobot.Robot;
using Blazor.Quartz.Common;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;
using Blazor.Quartz.Core.Service.Timer.Dto;
using Microsoft.AspNetCore.Cors;
using Blazor.Quartz.Core.Const;
using Blazor.Quartz.Core.Dapper;
using Blazor.Quartz.Core.Entity;
using Blazor.Quartz.Core.Service.App.Dto;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using Blazor.Quartz.Core.Service.App.Enum;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Blazor.Quartz.Web.Controllers
{
    /// <summary>
    /// 任务调度
    /// </summary>
    [Route("[controller]/[Action]")]
    [EnableCors("AllowSameDomain")] //允许跨域 
    public class WorkController : Controller
    {
        /// <summary>
        /// 检查心跳
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CheckJob()
        {
            HttpResultModel httpResultModel = new HttpResultModel();
            try
            {
                var api = AppConfig.ApiHost + "/healthcheck";
                var res = await api.GetStringAsync();
                if (res.ToLower() != "ok")
                {
                    await DingTalkRobot.SendTextMessage($"【心跳检查服务】任务调度状态异常，请检查", null, false);
                }
                httpResultModel.resData = res;
                httpResultModel.isSuccess = true;
                httpResultModel.resMsg = "【执行成功】";
            }
            catch (FlurlHttpException ex)
            {
                httpResultModel.resData = JsonConvert.SerializeObject(ex);
                httpResultModel.isSuccess = false;
                httpResultModel.resMsg = "【异常】";
                await DingTalkRobot.SendTextMessage($"【心跳检查】【异常】消息:{ex.Message}", null, false);
            }
            return new JsonResult(httpResultModel);
        }

        /// <summary>
        /// 每日报表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DailyReport()
        {
            HttpResultModel httpResultModel = new HttpResultModel();
            try
            {
                #region 每日报表
                var sql = $"SELECT * FROM {QuartzConstant.TablePrefix}JOB_DETAILS WHERE 1=1 ORDER BY JOB_GROUP,JOB_NAME ASC";
                var dynamicParams = new DynamicParameters();
                var jobRes = await DbContext.QueryAsync<JOB_DETAILS>(sql, dynamicParams);
                var jobList = jobRes.OrderBy(o => o.JOB_GROUP).ToList();

                var jobLogRes = await DbContext.QueryAsync<JOB_EXECUTION_LOG>($@"SELECT [JOB_NAME]
                            ,[JOB_GROUP]
                            ,[EXECUTION_STATUS]
                            ,[REQUEST_URL]
                            ,[REQUEST_TYPE]
                            ,[HEADERS]
                            ,[REQUEST_DATA]
                            ,[RESPONSE_DATA]
                            ,[BEGIN_TIME] FROM {QuartzConstant.TablePrefix}JOB_EXECUTION_LOG WHERE BEGIN_TIME >= @START_TIME AND BEGIN_TIME<= @END_TIME ORDER BY BEGIN_TIME DESC", new { START_TIME = DateTime.Now.AddDays(-1).Date, END_TIME = DateTime.Now.Date.AddMilliseconds(-1) });
                var jobLogList = jobLogRes.ToList();
                //钉钉通知
                MDMessageModel messageModel = new MDMessageModel();
                messageModel.Title = $"任务调度{DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd")}日报表";
                var msList = new List<string>();

                foreach (var item in jobRes)
                {
                    var thisJobLogList = jobLogList.Where(o => o.JOB_NAME == item.JOB_NAME && o.JOB_GROUP == item.JOB_GROUP).ToList();
                    var errCount = thisJobLogList.Count(o => o.EXECUTION_STATUS == ExecutionStatusEnum.Failure);
                    var thisMsg = $"{item.JOB_GROUP}-{item.JOB_NAME} 执行{thisJobLogList.Count()}次 异常{errCount}次";
                    msList.Add(thisMsg);
                    Console.WriteLine(thisMsg);
                }
                messageModel.Text = msList;
                await DingTalkRobot.SendMarkdownMessageByModel(messageModel, null, false);
                #endregion
                httpResultModel.resData = "";
                httpResultModel.isSuccess = true;
                httpResultModel.resMsg = "【执行成功】";
            }
            catch (Exception ex)
            {
                httpResultModel.resData = JsonConvert.SerializeObject(ex);
                httpResultModel.isSuccess = false;
                httpResultModel.resMsg = $"【异常】";
                await DingTalkRobot.SendTextMessage($"【任务调度每日报表】【异常】消息:{ex.Message}", null, false);
            }
            return new JsonResult(httpResultModel);
        }

        /// <summary>
        /// 清理日志
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ClearnLog()
        {
            HttpResultModel httpResultModel = new HttpResultModel();
            try
            {
                #region 清理数据
                if (AppConfig.AutoClearnLog)
                {
                    //清理数据
                    var RunLogStorageDays = AppConfig.RunLogStorageDays;
                    string startTime = DateTime.Now.AddDays(-Convert.ToInt32(RunLogStorageDays)).Date.ToString("yyyy-MM-dd");
                    await DbContext.ExecuteAsync($@"DELETE FROM {QuartzConstant.TablePrefix}JOB_EXECUTION_LOG WHERE BEGIN_TIME<@START_TIME", new { START_TIME = startTime });
                }
                #endregion
                httpResultModel.resData = "";
                httpResultModel.isSuccess = true;
                httpResultModel.resMsg = "【执行成功】";
            }
            catch (Exception ex)
            {
                httpResultModel.resData = JsonConvert.SerializeObject(ex);
                httpResultModel.isSuccess = false;
                httpResultModel.resMsg = $"【异常】";
                await DingTalkRobot.SendTextMessage($"【清理日志】【异常】消息:{ex.Message}", null, false);
            }
            return new JsonResult(httpResultModel);
        }
    }
}
