using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies(ILogger logger) : IUsersMicroservicePolicies
{
    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicies()
    {
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(retryCount: 5,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
                }
            );
        return policy;
    }
}