using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Service
{
    public class StartService
    {
        public void Start()
        {
            // 开启调度
            ISchedulerFactory sf = new StdSchedulerFactory();
            IScheduler scheduler = sf.GetScheduler().Result;
            IJobDetail job = JobBuilder.Create<CheckJob>().Build();
            // 服务启动时执行一次
            // ITrigger triggerNow = TriggerBuilder.Create().StartNow().Build();
            ITrigger trigger = TriggerBuilder.Create()
                .StartNow()
                .WithCronSchedule("0/30 * * * * ?")
                .Build();

            scheduler.ScheduleJob(job, trigger);
            scheduler.Start();

            IJobDetail job2 = JobBuilder.Create<ReportJob>().Build();
            // 服务启动时执行一次
            // ITrigger triggerNow = TriggerBuilder.Create().StartNow().Build();
            ITrigger trigger2 = TriggerBuilder.Create()
                .StartNow()
                .WithCronSchedule("0/30 * * * * ?")
                .Build();

            scheduler.ScheduleJob(job2, trigger2);
            scheduler.Start();
        }

        public void Stop()
        {
        }
    }
}
