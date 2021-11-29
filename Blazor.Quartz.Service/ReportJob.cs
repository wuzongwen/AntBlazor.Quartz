using Blazor.Quartz.Common;
using Blazor.Quartz.Common.DingTalkRobot.Robot;
using Blazor.Quartz.Core.Const;
using Blazor.Quartz.Core.Dapper;
using Blazor.Quartz.Core.Entity;
using Blazor.Quartz.Core.Service.App.Dto;
using Blazor.Quartz.Core.Service.App.Enum;
using Dapper;
using Newtonsoft.Json;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Service
{
    public class ReportJob : IJob
    {
        Task IJob.Execute(IJobExecutionContext context)
        {
            return Task.Run(async () =>
            {
                try
                {
                    #region 清理数据
                    //清理数据
                    var RunLogStorageDays = AppConfig.RunLogStorageDays;
                    if (RunLogStorageDays == null)
                    {
                        //默认保留7天
                        RunLogStorageDays = "7";
                    }
                    string startTime = DateTime.Now.AddDays(-Convert.ToInt32(RunLogStorageDays)).Date.ToString("yyyy-MM-dd");
                    await DbContext.ExecuteAsync($@"DELETE FROM {QuartzConstant.TablePrefix}JOB_EXECUTION_LOG WHERE BEGIN_TIME<@START_TIME", new { START_TIME = startTime });
                    #endregion

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
                            ,[BEGIN_TIME] FROM { QuartzConstant.TablePrefix}JOB_EXECUTION_LOG WHERE BEGIN_TIME >= @START_TIME AND BEGIN_TIME<= @END_TIME ORDER BY BEGIN_TIME DESC", new { START_TIME = DateTime.Now.AddDays(-1).Date, END_TIME = DateTime.Now.Date.AddMilliseconds(-1) });
                    var jobLogList = jobLogRes.ToList();
                    //钉钉通知
                    MDMessageModel messageModel = new MDMessageModel();
                    messageModel.Title = $"任务调度{DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd")}日报表";
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"任务调度{DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd")}日报表");
                    var msList = new List<string>();

                    foreach (var item in jobRes)
                    {
                        var thisJobLogList = jobLogList.Where(o => o.JOB_NAME == item.JOB_NAME && o.JOB_GROUP == item.JOB_GROUP).ToList();
                        var errCount = thisJobLogList.Count(o => o.EXECUTION_STATUS == ExecutionStatusEnum.Failure);
                        var thisMsg = $"{item.JOB_GROUP}-{item.JOB_NAME} 执行{thisJobLogList.Count()}次 异常{errCount}次";
                        msList.Add(thisMsg);
                        Console.WriteLine(thisMsg);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    messageModel.Text = msList;
                    await DingTalkRobot.SendMarkdownMessageByModel(messageModel, null, false);
                    #endregion
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "【任务调度每日报表】【异常】");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"【任务调度每日报表】【异常】消息:{ex.Message}");
                    Console.ForegroundColor = ConsoleColor.White;
                    await DingTalkRobot.SendTextMessage($"【任务调度每日报表】【异常】消息:{ex.Message}", null, false);
                }
                finally 
                {
                    Console.WriteLine("************************************************************************************************************************");
                }
            });
        }
    }
}
