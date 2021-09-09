using Blazor.Quartz.Common.PollyClient;
using Blazor.Quartz.Core.Hubs;
using Blazor.Quartz.Core.Service.App;
using Blazor.Quartz.Core.Service.Timer;
using Blazor.Quartz.Web.Extensions;
using Flurl.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Blazor.Quartz.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //心跳检查
            services.AddHealthChecks();

            // 日志配置
            LogConfig();

            #region 跨域     
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSameDomain", policyBuilder =>
                {
                    policyBuilder
                        .AllowAnyMethod()
                        .AllowAnyHeader();

                    var allowedHosts = Configuration.GetSection("AllowedHosts").Get<List<string>>();
                    if (allowedHosts?.Any(t => t == "*") ?? false)
                        policyBuilder.AllowAnyOrigin(); //允许任何来源的主机访问
                    else if (allowedHosts?.Any() ?? false)
                        policyBuilder.AllowCredentials().WithOrigins(allowedHosts.ToArray()); //允许类似http://localhost:8080等主机访问
                });
            });
            #endregion

            //注册AntDesign组件
            services.AddAntDesign();

            //注入SignalR实时通讯，默认用json传输
            services.AddSignalR(options =>
            {
                //客户端发保持连接请求到服务端最长间隔，默认30秒，改成4分钟，网页需跟着设置connection.keepAliveIntervalInMilliseconds = 12e4;即2分钟
                //options.ClientTimeoutInterval = TimeSpan.FromMinutes(4);
                //服务端发保持连接请求到客户端间隔，默认15秒，改成2分钟，网页需跟着设置connection.serverTimeoutInMilliseconds = 24e4;即4分钟
                //options.KeepAliveInterval = TimeSpan.FromMinutes(2);
            });

            //注册Cookie认证服务
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

            //Json中文
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            services.AddWebEncoders(opt =>
            {
                opt.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });

            //注入Polly重试服务
            services.AddSingleton<Policies>();

            //注入任务调度
            services.AddHostedService<QuartzService>();
            services.AddSingleton<SchedulerCenter>();

            //依赖注入
            services.AddSingleton<IAppService, AppService>();
            services.AddSingleton<IJobLogService, JobLogService>();

            //services.AddScoped(sp =>
            //    new HttpClient
            //    {
            //        BaseAddress = new Uri("http://www.baidu.com")
            //    });

            services.AddRazorPages();
            services.AddServerSideBlazor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //心跳检查
            app.UseHealthChecks("/healthCheck",
            new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                                await context.Response.WriteAsync("OK");
                }
            });

            //配置Flurl使用Polly实现重试Policy
            var policies = app.ApplicationServices.GetService<Policies>();
            FlurlHttp.Configure(setting =>
                        setting.HttpClientFactory = new PollyHttpClientFactory(policies));

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("AllowSameDomain");

            app.UseCookiePolicy();
            //注意app.UseAuthentication方法一定要放在下面的app.UseMvc方法前面，否者后面就算调用HttpContext.SignInAsync进行用户登录后，使用
            //HttpContext.User还是会显示用户没有登录，并且HttpContext.User.Claims读取不到登录用户的任何信息。
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapHub<NotifyHub>(NotifyHub.HubUrl);
            });
        }

        /// <summary>
        /// 日志配置
        /// </summary>      
        private void LogConfig()
        {
            //nuget导入
            //Serilog.Extensions.Logging
            //Serilog.Sinks.File
            //Serilog.Sinks.Async
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
        }
    }
}
