using WebApp.DTOs;

namespace WebApp.ApiClients
{
    public class PaymentApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoriesApiClient> _logger;

        public PaymentApiClient(HttpClient httpClient, ILogger<CategoriesApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<PaymentDto?>> GetAllPayments()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<PaymentDto>>("payment/get_all");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load payments from API.");
                throw;
            }
        }

        public async Task<PaymentDto?> CreatePaymentAsync(PaymentDto payment)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("payment/create", payment);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<PaymentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create payment with API client.");
                throw;
            }
        }
    }
}
