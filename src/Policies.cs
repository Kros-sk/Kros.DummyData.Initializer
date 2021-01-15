using Polly;
using Polly.Retry;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Polly.Timeout;
using System.Linq;
using Microsoft.Extensions.Logging;
using Polly.Extensions.Http;

namespace Kros.DummyData.Initializer
{
    internal static class Policies
    {
        private static AsyncPolicy<HttpResponseMessage> TimeoutePolicy(int timeoute, ILogger logger)
            => Policy.TimeoutAsync<HttpResponseMessage>(timeoute, (context, timeSpan, task) =>
                {
                    logger.LogWarning($"Delegate fired after {timeSpan.Seconds} seconds.");
                    return Task.CompletedTask;
                });

        private static AsyncRetryPolicy<HttpResponseMessage> RetryPolicy(IEnumerable<int> retry, ILogger logger)
            => HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(retry.Select(r=> TimeSpan.FromSeconds(r)),
                (delegateResult, retryCount) =>
                {
                    logger.LogWarning($"Retry delegate fired, attempt {retryCount}.");
                });

        public static AsyncPolicyWrap<HttpResponseMessage> PolicyStrategy(int timeoute, IEnumerable<int> retry, ILogger logger)
            => Policy.WrapAsync(RetryPolicy(retry, logger), TimeoutePolicy(timeoute, logger));
    }
}
