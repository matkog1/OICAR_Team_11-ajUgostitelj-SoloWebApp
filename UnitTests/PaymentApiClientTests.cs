using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using WebApp.ApiClients;
using WebApp.DTOs;
using Xunit;

public class PaymentApiClientTests
{
    private PaymentApiClient CreateClient(HttpResponseMessage response)
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
        return new PaymentApiClient(httpClient, logger);
    }

    [Fact]
    public async Task GetAllPayments_ReturnsList()
    {
        var payments = new List<PaymentDto>
        {
            new()
            {
                Id = 1,
                Amount = 12.50m,
                Method = "Credit Card",
                PaymentDate = DateTime.UtcNow,
                OrderId = 101
            },
            new()
            {
                Id = 2,
                Amount = 8.00m,
                Method = "Cash",
                PaymentDate = DateTime.UtcNow,
                OrderId = 102
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(payments)
        };
        var client = CreateClient(response);

        var result = await client.GetAllPayments();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p?.Method == "Cash");
        Assert.Contains(result, p => p?.OrderId == 101);
    }

    [Fact]
    public async Task CreatePaymentAsync_ReturnsCreatedPayment()
    {
        var paymentToCreate = new PaymentDto
        {
            Amount = 5.00m,
            Method = "Cash",
            PaymentDate = DateTime.UtcNow,
            OrderId = 105
        };

        var createdPayment = new PaymentDto
        {
            Id = 10,
            Amount = 5.00m,
            Method = "Cash",
            PaymentDate = paymentToCreate.PaymentDate,
            OrderId = 105
        };

        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(createdPayment)
        };

        var client = CreateClient(response);

        var result = await client.CreatePaymentAsync(paymentToCreate);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("Cash", result.Method);
        Assert.Equal(105, result.OrderId);
    }
}
