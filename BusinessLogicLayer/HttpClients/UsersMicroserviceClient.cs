using System.Net.Http.Json;
using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient(HttpClient httpClient)
{
    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
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
        return user;
    }
}