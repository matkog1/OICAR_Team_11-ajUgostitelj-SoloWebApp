using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using WebApp.ApiClients;
using WebApp.DTOs;
using Xunit;

public class ReviewApiClientTests
{
    private ReviewApiClient CreateClient(HttpResponseMessage response)
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
        return new ReviewApiClient(httpClient, logger);
    }


    [Fact]
    public async Task LoadReviewsAsync_ReturnsList()
    {
        var reviews = new List<ReviewDTO>
        {
            new()
            {
                Id = 1,
                ProductId = 10,
                ProductName = "Espresso",
                Rating = 5,
                Comment = "Excellent!",
                ReviewerName = "Pero",
                ReviewDate = DateTime.UtcNow
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(reviews)
        };
        var client = CreateClient(response);

        var result = await client.LoadReviewsAsync();

        Assert.Single(result);
        Assert.Equal("Espresso", result[0].ProductName);
        Assert.Equal("Pero", result[0].ReviewerName);
    }

    [Fact]
    public async Task LoadReviewAsync_ReturnsReview_WhenFound()
    {
        var review = new ReviewDTO
        {
            Id = 1,
            ProductId = 10,
            ProductName = "Caffe latte",
            Rating = 4,
            Comment = "Super",
            ReviewerName = "Ante",
            ReviewDate = DateTime.UtcNow
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(review)
        };
        var client = CreateClient(response);

        var result = await client.LoadReviewAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Caffe latte", result?.ProductName);
        Assert.Equal("Ante", result?.ReviewerName);
    }

    [Fact]
    public async Task LoadReviewAsync_ReturnsNull_WhenNotFound()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = CreateClient(response);

        var result = await client.LoadReviewAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateReviewAsync_ReturnsCreatedReview()
    {
        var newReview = new ReviewDTO
        {
            ProductId = 5,
            ProductName = "Coca cola",
            Rating = 5,
            Comment = "Klasika",
            ReviewerName = "Ana"
        };

        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new ReviewDTO
            {
                Id = 42,
                ProductId = 5,
                ProductName = "Coca cola",
                Rating = 5,
                Comment = "Klasika",
                ReviewerName = "Ana",
                ReviewDate = DateTime.UtcNow
            })
        };
        var client = CreateClient(response);

        var result = await client.CreateReviewAsync(newReview);

        Assert.Equal(42, result.Id);
        Assert.Equal("Coca cola", result.ProductName);
        Assert.Equal("Ana", result.ReviewerName);
    }

    [Fact]
    public async Task UpdateReviewAsync_ReturnsTrue_WhenSuccessful()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var client = CreateClient(response);

        var result = await client.UpdateReviewAsync(new ReviewDTO
        {
            Id = 1,
            ProductId = 1,
            ProductName = "Shake",
            Rating = 3,
            Comment = "Ok",
            ReviewerName = "Marina"
        });

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateReviewAsync_ReturnsFalse_WhenNotFound()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = CreateClient(response);

        var result = await client.UpdateReviewAsync(new ReviewDTO
        {
            Id = 999,
            ProductName = "Unknown",
            Comment = "N/A"
        });

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteReviewAsync_ReturnsTrue_WhenSuccessful()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var client = CreateClient(response);

        var result = await client.DeleteReviewAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteReviewAsync_ReturnsFalse_WhenNotFound()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = CreateClient(response);

        var result = await client.DeleteReviewAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task LoadReviewsByProductId_ReturnsList()
    {
        var reviews = new List<ReviewDTO>
        {
            new()
            {
                Id = 1,
                ProductId = 20,
                ProductName = "Macha",
                Rating = 4,
                Comment = "Good value",
                ReviewerName = "Eva",
                ReviewDate = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                ProductId = 20,
                ProductName = "Cheesecake",
                Rating = 3,
                Comment = "Average",
                ReviewerName = "Marko",
                ReviewDate = DateTime.UtcNow
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(reviews)
        };
        var client = CreateClient(response);

        var result = await client.LoadReviewsByProductId(20);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.ReviewerName == "Eva");
        Assert.Contains(result, r => r.Comment == "Average");
    }
}