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

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error creating payment: {ErrorContent}", errorContent);
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}. Error: {errorContent}");
                }
                return await response.Content.ReadFromJsonAsync<PaymentDto>();
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error occurred while creating payment.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while creating payment with API client.");
                throw;
            }
        }

    }
}
