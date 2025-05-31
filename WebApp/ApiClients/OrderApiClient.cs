using WebApp.DTOs;

namespace WebApp.ApiClients
{
    public class OrderApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoriesApiClient> _logger;

        public OrderApiClient(HttpClient httpClient, ILogger<CategoriesApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<OrderDto?> GetOrderById(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<OrderDto>($"order/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load order by id from API.");
                throw;
            }
        }

        public async Task<string?> GetOrderStatus(int? orderId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"order/{orderId}/status");
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load order status by id from API.");
                throw;
            }
        }
        public async Task<OrderDto?> CreateOrder(OrderDto newOrder)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("order", newOrder);
                response.EnsureSuccessStatusCode();
                var createdOrder = await response.Content.ReadFromJsonAsync<OrderDto>();
                return createdOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order with API.");
                throw;
            }
        }
        
        public async Task<bool> AddOrderItemsToOrder(List<OrderItemDto> newOrderItems)
        {
            try
            {
                foreach (var item in newOrderItems)
                {
                    var added = await _httpClient.PostAsJsonAsync("orderItems", item);
                    if (!added.IsSuccessStatusCode) return false;

                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add order items to order API.");
                throw;
            }
        }

    }
}
