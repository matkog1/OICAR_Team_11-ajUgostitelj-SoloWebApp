using WebApp.ApiClients;
using WebApp.DTOs;
using Xunit;

namespace WebApp.IntegrationTests
{
    public class ReviewApiClientIntegrationTests : IntegrationTestBase
    {
        private readonly ReviewApiClient _client;

        public ReviewApiClientIntegrationTests()
        {
            _client = new ReviewApiClient(HttpClient, Logger);
        }

        [Fact]
        public async Task LoadReviewsAsync_ReturnsSomeReviews()
        {
            var reviews = await _client.LoadReviewsAsync();

            Assert.NotNull(reviews);
            Assert.True(reviews.Count > 0);
        }

        [Fact]
        public async Task CreateReviewAsync()
        {
            var review = new ReviewDTO
            {
                ProductId = 19,
                ProductName = "Espresso",
                Rating = 5,
                Comment = "Excellent!",
                ReviewerName = "IntegrationTest",
                ReviewDate = DateTime.UtcNow
            };

            var created = await _client.CreateReviewAsync(review);

            Assert.NotNull(created);
            Assert.True(created.Id > 0);
            Assert.Equal("Espresso", created.ProductName);
        }
    }

}