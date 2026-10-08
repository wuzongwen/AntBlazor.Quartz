using Polly;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor.Quartz.Common.PollyClient
{
    public class Policies
    {
        private ResiliencePipeline<HttpResponseMessage> _pipeline;

        /// <summary>
        /// 超时+重试组合策略（每次尝试3秒超时；非200或超时后按10/30/60秒重试3次，第3次重试时发送钉钉通知）
        /// </summary>
        public ResiliencePipeline<HttpResponseMessage> PolicyStrategy
        {
            get
            {
                if (_pipeline == null)
                {
                    _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                        .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                        {
                            MaxRetryAttempts = 3,
                            DelayGenerator = args => ValueTask.FromResult<TimeSpan?>(args.AttemptNumber switch
                            {
                                0 => TimeSpan.FromSeconds(10),
                                1 => TimeSpan.FromSeconds(30),
                                _ => TimeSpan.FromSeconds(60)
                            }),
                            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                .HandleResult(r => r.StatusCode != HttpStatusCode.OK)
                                .Handle<TimeoutRejectedException>(),
                            OnRetry = args =>
                            {
                                if (args.AttemptNumber == 2)
                                {
                                    Task.Run(async() =>
                                    {
                                        await DingTalkRobot.Robot.DingTalkRobot.SendTextMessage($"外部接口请求异常:{args.Outcome.Exception}", null, false);
                                    });
                                }
                                return ValueTask.CompletedTask;
                            }
                        })
                        .AddTimeout(new TimeoutStrategyOptions
                        {
                            Timeout = TimeSpan.FromSeconds(3)
                        })
                        .Build();
                }
                return _pipeline;
            }
        }
    }

    public class PolicyHandler : DelegatingHandler
    {
        private readonly Policies _policies;

        public PolicyHandler(Policies policies)
        {
            _policies = policies;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await _policies.PolicyStrategy.ExecuteAsync(async ct => await base.SendAsync(request, ct), cancellationToken);
        }
    }

    /// <summary>
    /// Flurl 4 移除了 DefaultHttpClientFactory 扩展点，改由标准 IHttpClientFactory（AddHttpMessageHandler<PolicyHandler>）注册策略
    /// </summary>
    public class PollyHttpClientFactory
    {
        private readonly Policies _policies;

        public PollyHttpClientFactory(Policies policies)
        {
            _policies = policies;
        }

        /// <summary>
        /// 创建应用了超时/重试策略的 HttpClient
        /// </summary>
        public HttpClient CreateHttpClient()
        {
            return new HttpClient(new PolicyHandler(_policies)
            {
                InnerHandler = new HttpClientHandler()
            });
        }
    }
}
