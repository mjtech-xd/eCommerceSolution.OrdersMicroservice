using System.Net.Http.Json;
using System.Text.Json;
using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient(
    HttpClient httpClient,
    ILogger<UsersMicroserviceClient> logger,
    IDistributedCache distributedCache)
{
    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
        try
        {
            string cacheKeyToRead = $"user:{userID}";
            string? cachedUses = await distributedCache.GetStringAsync(cacheKeyToRead);

            if (cachedUses is not null)
            {
                //Deserialize the user data from cache and return it
                UserDTO? userFromCache = JsonSerializer.Deserialize<UserDTO>(cachedUses);
                return userFromCache;
            }

            HttpResponseMessage response = await httpClient.GetAsync($"/api/users/{userID}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    //throw new HttpRequestException($"Http request failed with the status code {response.StatusCode}");
                    return new UserDTO(PersonName: "Temporarily Unavailable",
                        Email: "Temporarily Unavailable",
                        Gender: "Temporarily Unavailable",
                        UserID: Guid.Empty);
                }
            }

            UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();
            if (user is null)
                throw new ArgumentException("Invalid user ID");

            //Store the user data into Cache
            string cacheKey = $"user:{userID}";
            string userJson = JsonSerializer.Serialize(user);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.UtcNow.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));
            await distributedCache.SetStringAsync(cacheKey, userJson, options);
            return user;
        }
        catch (BrokenCircuitException ex)
        {
            logger.LogInformation(ex,
                "Request failed because of circuit breaker is in open state. Returning dummy data.");
            return new UserDTO(PersonName: "Temporarily Unavailable",
                Email: "Temporarily Unavailable",
                Gender: "Temporarily Unavailable",
                UserID: Guid.Empty);
        }
    }
}