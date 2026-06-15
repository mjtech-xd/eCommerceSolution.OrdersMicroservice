using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger) : IUsersMicroservicePolicies
{
    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicies()
    {
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy<HttpResponseMessage>
            .HandleResult(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Use structured logging to avoid issues with interpolated strings
                    logger.LogInformation("Retry {Attempt} after {Seconds} seconds", retryAttempt, timespan.TotalSeconds);
                }
            );

        return policy;
    }
}