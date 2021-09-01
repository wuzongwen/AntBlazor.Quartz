using Blazor.Quartz.Core.Service.Timer;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor.Quartz.Web.Extensions
{
    public class QuartzService : IHostedService
    {
        private SchedulerCenter schedulerCenter;

        public QuartzService(SchedulerCenter schedulerCenter)
        {
            this.schedulerCenter = schedulerCenter;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //开启调度器
            await schedulerCenter.StartScheduleAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
