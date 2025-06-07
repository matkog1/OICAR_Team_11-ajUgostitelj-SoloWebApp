using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
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
                var requestJson = JsonSerializer.Serialize(newOrder);
                _logger.LogInformation("Creating order.., {RequestJson}", requestJson);

                var response = await _httpClient.PostAsJsonAsync("order", newOrder);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed creating order... {StatusCode}, Error: {ErrorContent}",
                        response.StatusCode, errorContent);
                }
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

        //moras ubacit delete order
        //fali nam i upate ordera moramo za integracijske testove

        public async Task<bool> DeleteOrderAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"order/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed deleting... {StatusCode}, Error: {ErrorContent}",
                        response.StatusCode, errorContent);
                }

                response.EnsureSuccessStatusCode();

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete order with API.");
                throw;
            }
        }
        public async Task<OrderDto?> UpdateOrderAsync(OrderDto updatedOrder)
        {
            try
            {
                var requestJson = JsonSerializer.Serialize(updatedOrder);
                _logger.LogInformation("Updating... {RequestJson}", requestJson);

                var response = await _httpClient.PutAsJsonAsync($"order/{updatedOrder.Id}", updatedOrder);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed update....: {StatusCode}, Error: {ErrorContent}",
                        response.StatusCode, errorContent);
                }

                response.EnsureSuccessStatusCode();
                return updatedOrder;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error when updating order.");
                throw;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update order with API.");
                throw;
            }
        }

    }
}
