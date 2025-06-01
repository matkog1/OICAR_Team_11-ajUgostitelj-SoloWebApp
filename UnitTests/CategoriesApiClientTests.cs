using System.Net;
using System.Net.Http.Json;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using WebApp.ApiClients;
using WebApp.DTOs;
using Xunit;

public class CategoriesApiClientTests
{
    private CategoriesApiClient CreateClient(HttpResponseMessage response)
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
        return new CategoriesApiClient(httpClient, logger);
    }

    [Fact]
    public async Task LoadCategoriesAsync_ReturnsList()
    {
        // Arrange
        var categories = new List<CategoryDto> { new() { Id = 1, Name = "Test" } };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(categories)
        };
        var client = CreateClient(response);

        // Act
        var result = await client.LoadCategoriesAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Test", result[0].Name);
    }

    [Fact]
    public async Task LoadCategoryAsync_ReturnsCategory()
    {
        var category = new CategoryDto { Id = 1, Name = "Test" };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(category)
        };
        var client = CreateClient(response);

        var result = await client.LoadCategoryAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Test", result?.Name);
    }

    [Fact]
    public async Task CreateCategoryAsync_ReturnsCreatedCategory()
    {
        var newCategory = new CategoryDto { Name = "New" };
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new CategoryDto { Id = 5, Name = "New" })
        };
        var client = CreateClient(response);

        var result = await client.CreateCategoryAsync(newCategory);

        Assert.Equal("New", result.Name);
        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ReturnsTrue_WhenSuccessful()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var client = CreateClient(response);

        var result = await client.UpdateCategoryAsync(new CategoryDto { Id = 1, Name = "Updated" });

        Assert.True(result);
    }


    [Fact]
    public async Task DeleteCategoryAsync_ReturnsTrue_WhenSuccessful()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var client = CreateClient(response);

        var result = await client.DeleteCategoryAsync(1);

        Assert.True(result);
    }
}
