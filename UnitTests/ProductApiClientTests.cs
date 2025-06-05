using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using WebApp.ApiClients;
using WebApp.DTOs;
using Xunit;

public class ProductApiClientTests
{
    private ProductApiClient CreateClient(HttpResponseMessage response)
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
        return new ProductApiClient(httpClient, logger);
    }
    [Fact]
    public async Task LoadProductsAsync_ReturnsList()
    {
        var products = new List<ProductDto>
        {
            new()
            {
                Id = 1,
                Name = "Cappuccino",
                Description = "Espresso with steamed milk and foam",
                Price = 3.50m,
                CategoryId = 1,
                CategoryName = "Coffee",
                Quantity = 50,
                ImageUrl = "/images/cappuccino.jpg",
                AverageRating = 4.7
            },
            new()
            {
                Id = 2,
                Name = "Cheesecake",
                Description = "Creamy New York-style cheesecake",
                Price = 4.25m,
                CategoryId = 3,
                CategoryName = "Cake",
                Quantity = 15,
                ImageUrl = "/images/cheesecake.jpg",
                AverageRating = 4.9
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(products)
        };
        var client = CreateClient(response);

        var result = await client.LoadProductsAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Cappuccino");
        Assert.Contains(result, p => p.CategoryName == "Cake");
    }

    [Fact]
    public async Task LoadProductAsync_ReturnsProduct_WhenFound()
    {
        var product = new ProductDto
        {
            Id = 1,
            Name = "Green Tea",
            Description = "Freshly brewed green tea",
            Price = 2.75m,
            CategoryId = 2,
            CategoryName = "Tea",
            Quantity = 30,
            ImageUrl = "/images/greentea.jpg",
            AverageRating = 4.4
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(product)
        };
        var client = CreateClient(response);

        var result = await client.LoadProductAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Green Tea", result?.Name);
        Assert.Equal("Tea", result?.CategoryName);
    }

    [Fact]
    public async Task LoadProductAsync_ReturnsNull_WhenNotFound()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = CreateClient(response);

        var result = await client.LoadProductAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateProductAsync_ReturnsCreatedProduct()
    {
        var newProduct = new ProductDto
        {
            Name = "Iced Latte",
            Description = "Cold coffee with milk",
            Price = 3.80m,
            CategoryId = 1,
            CategoryName = "Coffee",
            Quantity = 40,
            ImageUrl = "/images/icedlatte.jpg"
        };

        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new ProductDto
            {
                Id = 10,
                Name = "Iced Latte",
                Description = "Cold coffee with milk",
                Price = 3.80m,
                CategoryId = 1,
                CategoryName = "Coffee",
                Quantity = 40,
                ImageUrl = "/images/icedlatte.jpg",
                AverageRating = 4.5
            })
        };

        var client = CreateClient(response);

        var result = await client.CreateProductAsync(newProduct);

        Assert.Equal(10, result.Id);
        Assert.Equal("Iced Latte", result.Name);
        Assert.Equal(4.5, result.AverageRating);
    }

    [Fact]
    public async Task UpdateProductAsync_ReturnsTrue_WhenSuccessful()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var client = CreateClient(response);

        var result = await client.UpdateProductAsync(new ProductDto
        {
            Id = 2,
            Name = "Espresso",
            Description = "Strong black coffee",
            Price = 2.00m,
            CategoryId = 1,
            CategoryName = "Coffee",
            Quantity = 100
        });

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateProductAsync_ReturnsFalse_WhenNotFound()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = CreateClient(response);

        var result = await client.UpdateProductAsync(new ProductDto
        {
            Id = 999,
            Name = "Fejk prozivod"
        });

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteProductAsync_ReturnsTrue_WhenSuccessful()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var client = CreateClient(response);

        var result = await client.DeleteProductAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteProductAsync_ReturnsFalse_WhenNotFound()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = CreateClient(response);

        var result = await client.DeleteProductAsync(999);

        Assert.False(result);
    }
}
