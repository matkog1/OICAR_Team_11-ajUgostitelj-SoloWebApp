using System;
using System.Linq;
using System.Threading.Tasks;
using WebApp.ApiClients;
using WebApp.DTOs;
using Xunit;

namespace WebApp.IntegrationTests
{
    public class CategoriesApiClientIntegrationTests : IntegrationTestBase
    {
        private readonly CategoriesApiClient _client;

        public CategoriesApiClientIntegrationTests()
        {
            _client = new CategoriesApiClient(HttpClient, Logger);
        }

        [Fact]
        public async Task LoadCategoriesAsync()
        {
            var categories = await _client.LoadCategoriesAsync();
            Assert.NotNull(categories);
            Assert.True(categories.Count > 0, "Categories found!");
        }

        [Fact]
        public async Task LoadCategoryAsync_BadId()
        {
            var result = await _client.LoadCategoryAsync(-1234);
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateCategoryAsync()
        {
            var name = "Kategorija integracijskog testa";
            // arrange
            var toCreate = new CategoryDto
            {
                Name = name,
                ProductCount = 1
            };

            // act
            var created = await _client.CreateCategoryAsync(toCreate);

            // assert
            Assert.NotNull(created);
            Assert.True(created.Id > 0);
            Assert.Equal(name, created.Name);

            await _client.DeleteCategoryAsync(created.Id);
        }

        [Fact]
        public async Task CrudCategories()
        {
            // CREATE
            var name = $"Integracijski-test{Guid.NewGuid()}";
            var created = await _client.CreateCategoryAsync(new CategoryDto { Name = name, ProductCount = 69 });
            Assert.NotNull(created);

            try
            {
                // loadanje
                var fetched = await _client.LoadCategoryAsync(created.Id);
                Assert.NotNull(fetched);
                Assert.Equal(name, fetched!.Name);

                // update
                var name1 = name + "update test";
                created.Name = name1;
                var ok1 = await _client.UpdateCategoryAsync(created);
                Assert.True(ok1);

                var fetched2 = await _client.LoadCategoryAsync(created.Id);
                Assert.Equal(name1, fetched2!.Name);

                // brisanje
                var ok2 = await _client.DeleteCategoryAsync(created.Id);
                Assert.True(ok2);

                var fetched3 = await _client.LoadCategoryAsync(created.Id);
                Assert.Null(fetched3);
            }
            finally
            {
                await _client.DeleteCategoryAsync(created.Id);
            }
        }

        [Fact]
        public async Task UpdateCategoryFails()
        {
            var fake = new CategoryDto { Id = -9999, Name = "X", ProductCount = 0 };
            var result = await _client.UpdateCategoryAsync(fake);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteCategoryAsyncFails()
        {
            var result = await _client.DeleteCategoryAsync(-8888);
            Assert.False(result);
        }
    }


}