using Blazor.Quartz.Common;
using Blazor.Quartz.Common.DingTalkRobot.Robot;
using Quartz;
using Quartz.Impl;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Blazor.Quartz.Service
{
    public class StartService : ServiceControl, ServiceSuspend
    {
        private readonly IScheduler scheduler;

        public StartService()
        {
            scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
        }

        public bool Start(HostControl hostControl)
        {
            try
            {
                // 开启调度
                ISchedulerFactory sf = new StdSchedulerFactory();

                //检查心跳任务
                IJobDetail job = JobBuilder.Create<CheckJob>().Build();
                // 服务启动时执行一次
                // ITrigger triggerNow = TriggerBuilder.Create().StartNow().Build();
                ITrigger trigger = TriggerBuilder.Create()
                    .StartNow()
                    .WithCronSchedule(AppConfig.CheckJobCron)
                    .Build();
                scheduler.ScheduleJob(job, trigger);

                //每日报表任务
                IJobDetail job2 = JobBuilder.Create<ReportJob>().Build();
                // 服务启动时执行一次
                // ITrigger triggerNow = TriggerBuilder.Create().StartNow().Build();
                ITrigger trigger2 = TriggerBuilder.Create()
                    .StartNow()
                    .WithCronSchedule(AppConfig.ReportJobCron)
                    .Build();
                scheduler.ScheduleJob(job2, trigger2);

                //清理日志任务
                IJobDetail job3 = JobBuilder.Create<ClearnLogJob>().Build();
                // 服务启动时执行一次
                // ITrigger triggerNow = TriggerBuilder.Create().StartNow().Build();
                ITrigger trigger3 = TriggerBuilder.Create()
                    .StartNow()
                    .WithCronSchedule(AppConfig.ClearnLogJobCron)
                    .Build();
                scheduler.ScheduleJob(job3, trigger3);

                scheduler.Start();
            }
            catch (Exception ex)
            {
                Task.Run(async () =>
                {
                    await DingTalkRobot.SendTextMessage($"【{AppConfig.DisplayName}】【异常】消息:{ex.Message}", null, false);
                });
            }
            Log.Information($"{AppConfig.DisplayName}已启动");
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            scheduler.Shutdown(false);
            Log.Information($"{AppConfig.DisplayName}已停止");
            return true;
        }

        public bool Continue(HostControl hostControl)
        {
            scheduler.ResumeAll();
            Log.Information($"{AppConfig.DisplayName}已继续");
            return true;
        }

        public bool Pause(HostControl hostControl)
        {
            scheduler.PauseAll();
            Log.Information($"{AppConfig.DisplayName}已暂停");
            return true;
        }
    }
}
