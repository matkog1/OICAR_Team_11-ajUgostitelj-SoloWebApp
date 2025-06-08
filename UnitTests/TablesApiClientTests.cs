using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.ApiClients;
using System.Net.Http.Json;
using System.Net;
using WebApp.DTOs;

namespace UnitTests
{
    public class TablesApiClientTests
    {
        private TablesApiClient CreateClient(HttpResponseMessage response)
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

            var logger = new LoggerFactory().CreateLogger<TablesApiClient>();
            return new TablesApiClient(httpClient, logger);
        }

        [Fact]
        public async Task LoadTablesAsync_ReturnsList()
        {
            // Arrange
            var tables = new List<TableDto> { new() { Id = 1, Name = "Testni stol" } };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(tables)
            };
            var client = CreateClient(response);

            // Act
            var result = await client.LoadTablesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Testni stol", result[0].Name);
        }

        [Fact]
        public async Task LoadReviewAsync_ReturnsReview_WhenFound()
        {
            var table = new TableDto
            {
                Id = 1,
                Name = "Test stol",
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(table)
            };
            var client = CreateClient(response);

            var result = await client.LoadTableByIdAsync(1);

            Assert.NotNull(result);
        }
    }
}
