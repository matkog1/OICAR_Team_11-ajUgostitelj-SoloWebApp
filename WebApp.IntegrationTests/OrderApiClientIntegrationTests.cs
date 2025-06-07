using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApp;
using WebApp.ApiClients;
using WebApp.DTOs;
using Xunit;

public class OrderApiClientIntegrationTests : IntegrationTestBase
{
    private readonly OrderApiClient _client;
    private readonly ILogger<CategoriesApiClient> _logger;

    public OrderApiClientIntegrationTests()
    {
        _client = new OrderApiClient(HttpClient, Logger);
    }


    [Fact]
    public async Task GetOrderStatus()
    {
        var id = 1;
        var result = await _client.GetOrderStatus(id);
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateOrderAndDeleteOrder()
    {

        var createOrder = new OrderDto
        {
            OrderDate = DateTime.Now,
            Status = (int)OrderStatus.Paid,
            TotalAmount = 9,
            TableId = 2
        };

        var createdOrder = await _client.CreateOrder(createOrder);

        Assert.NotNull(createdOrder);
        Assert.True(createdOrder.Id > 0);

        Assert.Equal(createOrder.TableId, createdOrder.TableId); 
        Assert.Equal(createOrder.TotalAmount, createdOrder.TotalAmount);

        var deleteSuccess = await _client.DeleteOrderAsync(createdOrder.Id);

        Assert.True(deleteSuccess, "order deleted!");
    }

    [Fact]
    public async Task CompleteCRUDOrder()
    {
        //novi order
        var createdOrder = await _client.CreateOrder(
        new OrderDto
        {
            OrderDate = DateTime.Now,
            Status = (int)OrderStatus.Paid,
            TotalAmount = 9,
            TableId = 2
        }
        );
        Assert.NotNull(createdOrder);
        Assert.True(createdOrder.Id > 0); 

        var fetchedOrder = await _client.GetOrderById(createdOrder.Id);
        
        //usporedba dohvacenog i napravljenog
        Assert.NotNull(fetchedOrder);
        Assert.Equal(createdOrder.Id, fetchedOrder!.Id); 
        Assert.Equal(createdOrder.TotalAmount, fetchedOrder.TotalAmount);

        // update dohvacenog ordera
        var updatedOrder = new OrderDto
        {
            Id = fetchedOrder.Id, 
            OrderDate = fetchedOrder.OrderDate,
            Status = (int)OrderStatus.Completed, 
            TotalAmount = 15, 
            TableId = 3 
        };

        var responseFromUpdatedOrder = await _client.UpdateOrderAsync(updatedOrder);
        
        //provjera 
        Assert.NotNull(updatedOrder); 
        Assert.Equal(responseFromUpdatedOrder.Id, updatedOrder!.Id); 
        Assert.Equal(responseFromUpdatedOrder.Status, updatedOrder.Status); 
        Assert.Equal(responseFromUpdatedOrder.TotalAmount, updatedOrder.TotalAmount); 
        Assert.Equal(responseFromUpdatedOrder.TableId, updatedOrder.TableId); 

        //dohvacamo novi updejtani order
        var fetchedUpdatedOrder = await _client.GetOrderById(updatedOrder.Id);

        //brisanje
        var deleteSuccess = await _client.DeleteOrderAsync(createdOrder.Id);
        Assert.True(deleteSuccess, "The order should be deleted successfully");
    }
}
