using System.Net.Http.Json;
using System.Text.Json;
using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;

namespace BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient(HttpClient httpClient, IDistributedCache  distributedCache)
{
    public async Task<ProductDTO?> GetProductByProductID(Guid productID)
    {
        string cacheKey = $"product:{productID}";
        string? cachedProduct = await distributedCache.GetStringAsync(cacheKey);
        if (cachedProduct is not null)
        {
            ProductDTO? productFromCache = JsonSerializer.Deserialize<ProductDTO>(cachedProduct);
            return productFromCache;
        }
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
        
        //key: product:{productID}
        //Value: {"ProductName":.....,"Category":"...."}
        string productJson = JsonSerializer.Serialize(product);
        DistributedCacheEntryOptions options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
            .SetSlidingExpiration(TimeSpan.FromSeconds(100));
        string cacheKryToWrite = $"product:{productID}";
        await distributedCache.SetStringAsync(cacheKryToWrite, productJson, options);
        return product;
    }
}