using Blazor.Quartz.Common;
using Blazor.Quartz.Common.DingTalkRobot.Robot;
using Serilog;
using Serilog.Events;
using System;
using Topshelf;

namespace Blazor.Quartz.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileSize = 1024 * 1024 * 10;//10M
            var fileCount = 2;
            Log.Logger = new LoggerConfiguration()
                                 .Enrich.FromLogContext()
                                 .MinimumLevel.Debug()
                                 .MinimumLevel.Override("System", LogEventLevel.Information)
                                 .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                                 .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Debug).WriteTo.Async(
                                     a =>
                                     {
                                         a.File("logs/log-Debug-.txt", fileSizeLimitBytes: fileSize, retainedFileCountLimit: fileCount, rollingInterval: RollingInterval.Day);
                                     }
                                 ))
                                 .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Information).WriteTo.Async(
                                     a =>
                                     {
                                         a.File("logs/log-Info-.txt", fileSizeLimitBytes: fileSize, retainedFileCountLimit: fileCount, rollingInterval: RollingInterval.Day);
                                     }
                                 ))
                                 .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Warning).WriteTo.Async(
                                     a =>
                                     {
                                         a.File("logs/log-Warning-.txt", fileSizeLimitBytes: fileSize, retainedFileCountLimit: fileCount, rollingInterval: RollingInterval.Day);
                                     }
                                 ))
                                 .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.Async(
                                     a =>
                                     {
                                         a.File("logs/log-Error-.txt", fileSizeLimitBytes: fileSize, retainedFileCountLimit: fileCount, rollingInterval: RollingInterval.Day);
                                     }
                                 ))
                                 .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Fatal).WriteTo.Async(
                                     a =>
                                     {
                                         a.File("logs/log-Fatal-.txt", fileSizeLimitBytes: fileSize, retainedFileCountLimit: fileCount, rollingInterval: RollingInterval.Day);

                                     }
                                 ))
                                 //所有情况
                                 .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => true)).WriteTo.Async(
                                     a =>
                                     {
                                         a.File("logs/log-All-.txt", fileSizeLimitBytes: fileSize, retainedFileCountLimit: fileCount, rollingInterval: RollingInterval.Day);
                                     }
                                 )
                                .CreateLogger();

            HostFactory.Run(config =>
            {
                config.Service<StartService>();
                config.RunAsLocalSystem();
                config.EnableServiceRecovery(r => r.RestartService(TimeSpan.FromSeconds(10)));
                config.SetDescription(AppConfig.Description);
                config.SetServiceName(AppConfig.ServiceName);
                config.SetDisplayName(AppConfig.DisplayName);
                config.StartAutomatically();
                config.EnablePauseAndContinue();//支持暂停和继续
                config.EnableShutdown();//支持停止
                //异常处理
                config.OnException(ex =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"异常,信息详情:{ex.Message}");
                    Log.Error($"异常,信息详情:{ex.Message}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                });
            });
        }
    }
}
