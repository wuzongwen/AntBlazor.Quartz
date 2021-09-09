using Blazor.Quartz.Common;
using Blazor.Quartz.Common.DingTalkRobot.Robot;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Serilog;

namespace Blazor.Quartz.Service
{
    public class CheckJob : IJob
    {
        Task IJob.Execute(IJobExecutionContext context)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var api = AppConfig.ApiHost + "/healthcheck";
                    var res = await api.GetStringAsync();
                    Log.Information($"【心跳检查】请求心跳接口响应信息:{res}");
                    if (res.ToLower() != "ok") 
                    {
                        await DingTalkRobot.SendTextMessage($"【心跳检查服务】任务调度状态异常，请检查", null, false);
                    }
                    Log.Information($"【心跳检查】状态正常");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"【心跳检查】状态正常");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                catch (FlurlHttpException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"【心跳检查】【异常】消息:{ex.Message}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Log.Error($"【心跳检查】【异常】消息:{ex}");
                    await DingTalkRobot.SendTextMessage($"【心跳检查】【异常】消息:{ex.Message}", null, false);
                }
                finally
                {
                    Console.WriteLine("************************************************************************************************************************");
                }
            });  
        }
    }
}
