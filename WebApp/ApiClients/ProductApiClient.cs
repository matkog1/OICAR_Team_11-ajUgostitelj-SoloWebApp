using System.Net;
using WebApp.DTOs;

namespace WebApp.ApiClients
{
    public class ProductApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<CategoriesApiClient> _logger;

        public ProductApiClient(HttpClient http, ILogger<CategoriesApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public Task<List<ProductDto>> LoadProductsAsync()
        {
            try
            {
                return _http.GetFromJsonAsync<List<ProductDto>>("product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load products from API.");
                throw;
            }
        }
           
        public async Task<ProductDto?> LoadProductAsync(int id)
        {
            try
            {
                var resp = await _http.GetAsync($"product/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return null;
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadFromJsonAsync<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load product from API.");
                throw;
            }
        }

        public async Task<ProductDto> CreateProductAsync(ProductDto newProduct)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("product", newProduct);
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadFromJsonAsync<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create product via API.");
                throw;
            }
        }

        public async Task<bool> UpdateProductAsync(ProductDto updatedProduct)
        {
            try
            {
                var resp = await _http.PutAsJsonAsync($"product/{updatedProduct.Id}", updatedProduct);
                if (resp.StatusCode == HttpStatusCode.NotFound) return false;
                resp.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product via API.");
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var resp = await _http.DeleteAsync($"product/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return false;
                resp.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete product via API.");
                throw;
            }
        }
    }
}
