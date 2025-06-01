using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using WebApp.ApiClients;
using WebApp.DTOs;
using Xunit;

public class OrderApiClientTests
{
    private OrderApiClient CreateClient(HttpResponseMessage response)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var logger = new LoggerFactory().CreateLogger<CategoriesApiClient>();
        return new OrderApiClient(httpClient, logger);
    }

    [Fact]
    public async Task GetOrderById_ReturnsOrder()
    {
        // Arrange: mock a valid order
        var order = new OrderDto
        {
            Id = 1,
            OrderDate = DateTime.UtcNow,
            Status = "Completed",
            TotalAmount = 45.50m,
            TableId = 3
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(order)
        };
        var client = CreateClient(response);

        // Act
        var result = await client.GetOrderById(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result?.Id);
        Assert.Equal("Completed", result?.Status);
        Assert.Equal(45.50m, result?.TotalAmount);
        Assert.Equal(3, result?.TableId);
    }

    [Fact]
    public async Task GetOrderStatus_ReturnsStatus()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Processing")
        };
        var client = CreateClient(response);

        var result = await client.GetOrderStatus(1);

        Assert.Equal("Processing", result);
    }

    [Fact]
    public async Task GetOrderStatus_ReturnsNull_WhenNotSuccess()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var client = CreateClient(response);

        var result = await client.GetOrderStatus(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateOrder_ReturnsCreatedOrder()
    {
        
        var newOrder = new OrderDto
        {
            OrderDate = DateTime.UtcNow,
            Status = "Paid",
            TotalAmount = 22.99m,
            TableId = 1
        };

        var createdOrder = new OrderDto
        {
            Id = 10,
            OrderDate = newOrder.OrderDate,
            Status = "Paid",
            TotalAmount = 22.99m,
            TableId = 1
        };

        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(createdOrder)
        };
        var client = CreateClient(response);

        // Act
        var result = await client.CreateOrder(newOrder);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result?.Id);
        Assert.Equal("Paid", result?.Status);
        Assert.Equal(22.99m, result?.TotalAmount);
        Assert.Equal(1, result?.TableId);
    }

    
}
