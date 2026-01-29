using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;

namespace Blazor.Quartz.Core.Dependency
{
    public class ServiceProviderJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                var jobDetail = bundle.JobDetail;
                var jobType = jobDetail.JobType;
                // Use ActivatorUtilities to support constructor injection
                var job = ActivatorUtilities.CreateInstance(_serviceProvider, jobType) as IJob;
                return job;
            }
            catch (Exception ex)
            {
                throw new SchedulerException("Problem while instantiating job class", ex);
            }
        }

        public void ReturnJob(IJob job)
        {
            // Let the DI container handle disposal if the job is IDisposable
            if (job is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
