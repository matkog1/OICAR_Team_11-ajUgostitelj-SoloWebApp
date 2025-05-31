using Microsoft.Extensions.Logging;
using System.Net;
using WebApp.DTOs;

namespace WebApp.ApiClients
{
    public class CategoriesApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<CategoriesApiClient> _logger;

        public CategoriesApiClient(HttpClient http, ILogger<CategoriesApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<CategoryDto>> LoadCategoriesAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<CategoryDto>>("categories") ?? new();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to load categories from API.");
                throw;
            }
        }

        public async Task<CategoryDto?> LoadCategoryAsync(int id)
        {
            try
            {
                var resp = await _http.GetAsync($"categories/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return null;
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadFromJsonAsync<CategoryDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to load category from API.");
                throw;
            }
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryDto newCategory)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("categories", newCategory);
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadFromJsonAsync<CategoryDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed creating Category in Categories api client.");
                throw;
            }
        }

        public async Task<bool> UpdateCategoryAsync(CategoryDto updatedCategory)
        {
            try
            {
                var resp = await _http.PutAsJsonAsync($"categories/{updatedCategory.Id}", updatedCategory);
                if (resp.StatusCode == HttpStatusCode.NotFound) return false;
                resp.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed updating Category in Categories api client.");
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var resp = await _http.DeleteAsync($"categories/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return false;
                resp.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed deleting Category in Categories api client.");
                throw;
            }
        }
    }
}
