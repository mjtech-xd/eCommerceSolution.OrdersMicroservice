using Polly;

namespace BusinessLogicLayer.Policies;

public interface IUsersMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicies();
    IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicies();
}