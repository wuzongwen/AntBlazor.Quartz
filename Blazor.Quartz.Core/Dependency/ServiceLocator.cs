using System;

namespace Blazor.Quartz.Core.Dependency
{
    public static class ServiceLocator
    {
        public static IServiceProvider? ServiceProvider { get; set; }
    }
}
