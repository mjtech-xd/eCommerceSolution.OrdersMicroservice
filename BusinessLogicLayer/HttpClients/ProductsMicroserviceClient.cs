using System.Net.Http.Json;
using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient(HttpClient httpClient)
{
    public async Task<ProductDTO?> GetProductByProductID(Guid productID)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"/api/products/search/product-id/{productID}");
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
                throw new HttpRequestException($"Http request failed with the status code {response.StatusCode}");
            }
        }
        ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>();
        if (product is null)
            throw new ArgumentException("Invalid product ID");
        return product;
    }
}