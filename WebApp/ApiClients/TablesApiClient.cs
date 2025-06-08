using Microsoft.Extensions.Logging;
using System.Net;
using WebApp.DTOs;

namespace WebApp.ApiClients
{
    public class TablesApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<TablesApiClient> _logger;

        public TablesApiClient(HttpClient http, ILogger<TablesApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<TableDto>> LoadTablesAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<TableDto>>("table") ?? new();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to load tables from API.");
                throw;
            }
        }

        public async Task<TableDto?> LoadTableByIdAsync(int id)
        {
            try
            {
                var resp = await _http.GetAsync($"table/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return null;
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadFromJsonAsync<TableDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to load table by ID from API.");
                throw;
            }
        }

    }
}
