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
    public class ClearnLogJob : IJob
    {
        Task IJob.Execute(IJobExecutionContext context)
        {
            return Task.Run(async () =>
            {
                try
                {
                    #region 清理数据
                    if (AppConfig.AutoClearnLog)
                    {
                        //清理数据
                        var RunLogStorageDays = AppConfig.RunLogStorageDays;
                        string startTime = DateTime.Now.AddDays(-Convert.ToInt32(RunLogStorageDays)).Date.ToString("yyyy-MM-dd");
                        await DbContext.ExecuteAsync($@"DELETE FROM {QuartzConstant.TablePrefix}JOB_EXECUTION_LOG WHERE BEGIN_TIME<@START_TIME", new { START_TIME = startTime });
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"日志清理成功");
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "【任务调度清理数据】【异常】");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"【任务调度清理数据】【异常】消息:{ex.Message}");
                    Console.ForegroundColor = ConsoleColor.White;
                    await DingTalkRobot.SendTextMessage($"【任务调度清理数据】【异常】消息:{ex.Message}", null, false);
                }
                finally
                {
                    Console.WriteLine("************************************************************************************************************************");
                }
            });
        }
    }
}
