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
            //�������
            services.AddHealthChecks();

            // ��־����
            LogConfig();

            #region ����     
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSameDomain", policyBuilder =>
                {
                    policyBuilder
                        .AllowAnyMethod()
                        .AllowAnyHeader();

                    var allowedHosts = Configuration.GetSection("AllowedHosts").Get<List<string>>();
                    if (allowedHosts?.Any(t => t == "*") ?? false)
                        policyBuilder.AllowAnyOrigin(); //�����κ���Դ����������
                    else if (allowedHosts?.Any() ?? false)
                        policyBuilder.AllowCredentials().WithOrigins(allowedHosts.ToArray()); //��������http://localhost:8080����������
                });
            });
            #endregion

            //ע��AntDesign���
            services.AddAntDesign();

            //ע��SignalRʵʱͨѶ��Ĭ����json����
            services.AddSignalR(options =>
            {
                //�ͻ��˷������������󵽷����������Ĭ��30�룬�ĳ�4���ӣ���ҳ���������connection.keepAliveIntervalInMilliseconds = 12e4;��2����
                //options.ClientTimeoutInterval = TimeSpan.FromMinutes(4);
                //����˷������������󵽿ͻ��˼����Ĭ��15�룬�ĳ�2���ӣ���ҳ���������connection.serverTimeoutInMilliseconds = 24e4;��4����
                //options.KeepAliveInterval = TimeSpan.FromMinutes(2);
            });

            //ע��Cookie��֤����
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

            //Json����
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            services.AddWebEncoders(opt =>
            {
                opt.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });

            //ע��Polly���Է���
            services.AddSingleton<Policies>();

            //ע���������
            services.AddHostedService<QuartzService>();
            services.AddSingleton<SchedulerCenter>();

            //����ע��
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

            //�������
            app.UseHealthChecks("/healthCheck",
            new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                                await context.Response.WriteAsync("OK");
                }
            });

            //����Flurlʹ��Pollyʵ������Policy
            var policies = app.ApplicationServices.GetService<Policies>();
            FlurlHttp.Configure(setting =>
                        setting.HttpClientFactory = new PollyHttpClientFactory(policies));

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("AllowSameDomain");

            app.UseCookiePolicy();
            //ע��app.UseAuthentication����һ��Ҫ���������app.UseMvc����ǰ�棬���ߺ���������HttpContext.SignInAsync�����û���¼��ʹ��
            //HttpContext.User���ǻ���ʾ�û�û�е�¼������HttpContext.User.Claims��ȡ������¼�û����κ���Ϣ��
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
        /// ��־����
        /// </summary>      
        private void LogConfig()
        {
            //nuget����
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
                                 //�������
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
