using Blazor.Quartz.Common;
using Blazor.Quartz.Common.DingTalkRobot.Robot;
using Blazor.Quartz.Core.Const;
using Blazor.Quartz.Core.Dapper;
using Blazor.Quartz.Core.Service.App.Dto;
using Blazor.Quartz.Core.Service.App.Enum;
using Blazor.Quartz.Core.Service.Timer.Dto;
using Blazor.Quartz.Core.Service.Timer.Enum;
using Newtonsoft.Json;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Timer
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public abstract class JobBase<T> where T : LogModel, new()
    {
        protected readonly int warnTime = AppConfig.WarnTime;//接口请求超过多少秒记录警告日志 
        protected Stopwatch stopwatch = new Stopwatch();
        protected T LogInfo { get; private set; }

        public JobBase(T logInfo)
        {
            LogInfo = logInfo;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            //如果结束时间超过当前时间，则暂停当前任务。
            var endTime = context.JobDetail.JobDataMap.GetString("EndAt");
            if (!string.IsNullOrWhiteSpace(endTime) && DateTime.Parse(endTime) <= DateTime.Now)
            {
                await context.Scheduler.PauseJob(new JobKey(context.JobDetail.Key.Name, context.JobDetail.Key.Group));
                return;
            }

            //记录执行次数
            var runNumber = context.JobDetail.JobDataMap.GetLong(QuartzConstant.RUNNUMBER);
            context.JobDetail.JobDataMap[QuartzConstant.RUNNUMBER] = ++runNumber;

            stopwatch.Restart(); //  开始监视代码运行时间

            var model = new JOB_EXECUTION_LOG();
            try
            {
                LogInfo.BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                LogInfo.JobName = $"{context.JobDetail.Key.Group}.{context.JobDetail.Key.Name}";

                await NextExecute(context);
                model.RESPONSE_DATA = LogInfo.Res_Data;
            }
            catch (Exception ex)
            {
                LogInfo.ErrorMsg = $"<span class='error'>{ex.Message}</span>";
                context.JobDetail.JobDataMap[QuartzConstant.EXCEPTION] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
                await ErrorAsync(LogInfo.JobName, ex, JsonConvert.SerializeObject(LogInfo));
                model.RESPONSE_DATA = ex.Message;
            }
            finally
            {
                stopwatch.Stop(); //  停止监视            
                double seconds = stopwatch.Elapsed.TotalSeconds;  //总秒数             
                LogInfo.EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (seconds >= 1)
                    LogInfo.ExecuteTime = seconds + "秒";
                else
                    LogInfo.ExecuteTime = stopwatch.Elapsed.TotalMilliseconds + "毫秒";

                var classErr = string.IsNullOrWhiteSpace(LogInfo.ErrorMsg) ? "" : "error";
               
                if (seconds >= warnTime)//如果请求超过20秒，警告  
                {
                    await WarningAsync(LogInfo.JobName, "耗时过长 - " + JsonConvert.SerializeObject(LogInfo));
                }

                //添加执行记录
                model.JOB_NAME = context.JobDetail.Key.Name;
                model.JOB_GROUP = context.JobDetail.Key.Group;
                model.REQUEST_URL = LogInfo.Req_Url;
                model.REQUEST_TYPE = LogInfo.Req_Type;
                model.HEADERS = LogInfo.Headers;
                model.REQUEST_DATA = LogInfo.Result;
                model.BEGIN_TIME = LogInfo.BeginTime;
                model.EXECUTION_STATUS = LogInfo.Status;
                await DbContext.ExecuteAsync($@"INSERT INTO {QuartzConstant.TablePrefix}JOB_EXECUTION_LOG ([JOB_NAME]
                            ,[JOB_GROUP]
                            ,[EXECUTION_STATUS]
                            ,[REQUEST_URL]
                            ,[REQUEST_TYPE]
                            ,[HEADERS]
                            ,[REQUEST_DATA]
                            ,[RESPONSE_DATA]
                            ,[BEGIN_TIME]
                            ) VALUES(@JOB_NAME,@JOB_GROUP,@EXECUTION_STATUS,@REQUEST_URL,@REQUEST_TYPE,@HEADERS,@REQUEST_DATA,@RESPONSE_DATA,@BEGIN_TIME)", model);

                #region 清理数据
                if (AppConfig.AutoClearnLog.ToLower() == "true")
                {
                    //清理数据
                    var RunLogStorageDays = AppConfig.RunLogStorageDays;
                    if (RunLogStorageDays == null)
                    {
                        //默认保留7天
                        RunLogStorageDays = "7";
                    }
                    string startTime = DateTime.Now.AddDays(-Convert.ToInt32(RunLogStorageDays)).Date.ToString("yyyy-MM-dd");
                    await DbContext.ExecuteAsync($@"DELETE FROM {QuartzConstant.TablePrefix}JOB_EXECUTION_LOG WHERE BEGIN_TIME<@START_TIME", new { START_TIME = startTime });
                }
                #endregion
            }
        }

        public abstract Task NextExecute(IJobExecutionContext context);

        public async Task WarningAsync(string title, string msg)
        {
            Log.Logger.Warning(msg);
            await DingTalkRobot.SendTextMessage($"【警告】消息:{msg}", null, false);
        }

        public async Task InformationAsync(string title, string msg)
        {
            Log.Logger.Information(msg);
            await DingTalkRobot.SendTextMessage($"消息:{msg}", null, false);
        }

        public async Task ErrorAsync(string title, Exception ex, string msg)
        {
            Log.Logger.Error(ex, msg);
            await DingTalkRobot.SendTextMessage($"【异常】消息:{msg}", null, false);
        }
    }
}
