using Flurl.Http.Configuration;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
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
        /// <summary>
        /// 超时策略
        /// </summary>
        private AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy
        {
            get
            {
                return Policy.TimeoutAsync<HttpResponseMessage>(3, (context, span, task) =>
                {
                    //LoggerHelper.Info($"外部接口请求超时");
                    return Task.CompletedTask;
                });
            }
        }

        /// <summary>
        /// 重试策略
        /// </summary>
        private AsyncRetryPolicy<HttpResponseMessage> RetryPolicy
        {
            get
            {
                HttpStatusCode[] retryStatus =
                {
                    HttpStatusCode.OK
                };
                return Policy
                    .HandleResult<HttpResponseMessage>(r => !retryStatus.Contains(r.StatusCode))
                    .Or<TimeoutRejectedException>()
                    .WaitAndRetryAsync(new[]
                    {
                        // 表示重试3次，第一次1秒后重试，第二次2秒后重试，第三次4秒后重试
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(4)
                    }, (result, span, count, context) =>
                    {
                        if (count == 3)
                        {
                            //LoggerHelper.Warn($"外部接口请求异常:{result.Exception}");
                        }
                    });
            }
        }

        public AsyncPolicyWrap<HttpResponseMessage> PolicyStrategy =>
            Policy.WrapAsync(RetryPolicy, TimeoutPolicy);
    }

    public class PolicyHandler : DelegatingHandler
    {
        private readonly Policies _policies;

        public PolicyHandler(Policies policies)
        {
            _policies = policies;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _policies.PolicyStrategy.ExecuteAsync(ct => base.SendAsync(request, ct), cancellationToken);
        }
    }

    public class PollyHttpClientFactory : DefaultHttpClientFactory
    {
        private readonly Policies _policies;

        public PollyHttpClientFactory(Policies policies)
        {
            _policies = policies;
        }

        public override HttpMessageHandler CreateMessageHandler()
        {
            return new PolicyHandler(_policies)
            {
                InnerHandler = base.CreateMessageHandler()
            };
        }
    }
}
