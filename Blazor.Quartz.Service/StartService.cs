using Quartz;
using Quartz.Impl;
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
            // 开启调度
            ISchedulerFactory sf = new StdSchedulerFactory();
            IJobDetail job = JobBuilder.Create<CheckJob>().Build();
            // 服务启动时执行一次
            // ITrigger triggerNow = TriggerBuilder.Create().StartNow().Build();
            ITrigger trigger = TriggerBuilder.Create()
                .StartNow()
                .WithCronSchedule("0 0/5 * * * ?")
                .Build();

            scheduler.ScheduleJob(job, trigger);

            IJobDetail job2 = JobBuilder.Create<ReportJob>().Build();
            // 服务启动时执行一次
            // ITrigger triggerNow = TriggerBuilder.Create().StartNow().Build();
            ITrigger trigger2 = TriggerBuilder.Create()
                .StartNow()
                .WithCronSchedule("0 30 9 * * ?")
                .Build();

            scheduler.ScheduleJob(job2, trigger2);
            scheduler.Start();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            scheduler.Shutdown(false);
            return true;
        }

        public bool Continue(HostControl hostControl)
        {
            scheduler.ResumeAll();
            return true;
        }

        public bool Pause(HostControl hostControl)
        {
            scheduler.PauseAll();
            return true;
        }
    }
}
