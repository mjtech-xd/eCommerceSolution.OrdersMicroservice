using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
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

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicies()
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy<HttpResponseMessage>
            .HandleResult(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromMinutes(2),
                onBreak: (outcome, timespan) =>
                {
                    // Use structured logging to avoid issues with interpolated strings
                    logger.LogInformation("Circuit Breaker opened for {Minutes} minutes due to consecutive 3 failures. The subsequent requests will be blocked", timespan.TotalMinutes);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit Breaker reset");
                }
            );

        return policy;
    }
}