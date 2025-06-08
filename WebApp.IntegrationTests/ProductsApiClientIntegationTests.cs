using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebApp.ApiClients;
using WebApp.DTOs;
using Xunit;

namespace WebApp.IntegrationTests
{
    public class ProductsApiClientIntegrationTests : IntegrationTestBase
    {
        private readonly ProductApiClient _client;
        private readonly ILogger<ProductApiClient> _logger;
        private readonly CategoriesApiClient _categoriesApiClient;

        public ProductsApiClientIntegrationTests()
        {
            _client = new ProductApiClient(HttpClient, Logger);
        }

        [Fact]
        public async Task LoadProducts()
        {
            var products = await _client.LoadProductsAsync();
            Assert.NotNull(products);
            Assert.True(products.Count > 0, "Products found!");
        }

        [Fact]
        public async Task LoadProductById()
        {
            //kave su 19
            var id = 19;
            var result = await _client.LoadProductAsync(id);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task LoadProductByBadId()
        {
            var id = -334;
            var result = await _client.LoadProductAsync(id);
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateProductAsync()
        {
            //kave su 19 id pa cu njega uzet
            var name = "Product integracijskog testa";


            var createProduct = new ProductDto
            {
                Name = name,
                Description = name,
                Price = 99,
                CategoryId = 19,
                ImageUrl = "integracijskiTest.png",
                Quantity = 99
            };

            // act
            var created = await _client.CreateProductAsync(createProduct);

            // assert
            Assert.NotNull(created);
            Assert.True(created.Id > 0);
            Assert.Equal(name, created.Name);

            await _client.DeleteProductAsync(created.Id);
        }

        [Fact]
        public async Task CrudProductsAsync()
        {
            var name = $"Integracijski-test";

            var created = await _client.CreateProductAsync(
                new ProductDto
                {
                    Name = name,
                    Description = name,
                    Price = 99,
                    CategoryId = 19,
                    ImageUrl = "integracijskiTest.png",
                    Quantity = 99
                });
            Assert.NotNull(created);

            try
            {
                // napravimo product
                try
                {
                    created = await _client.CreateProductAsync(new ProductDto
                    {
                        Name = name,
                        Description = name,
                        Price = 99,
                        CategoryId = 19,
                        ImageUrl = "integracijskiTest.png",
                        Quantity = 99
                    });
                    Assert.NotNull(created);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed during CREATE step: " + ex.Message, ex);
                }

                // dohvat producta
                try
                {
                    var fetched = await _client.LoadProductAsync(created.Id);
                    Assert.NotNull(fetched);
                    Assert.Equal(name, fetched.Name);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed during LOAD step: " + ex.Message, ex);
                }

                // updatemo ga
                try
                {
                    var newName = name + " update test products";
                    created.Name = newName;

                    var ok = await _client.UpdateProductAsync(created);
                    Assert.True(ok);

                    var fetched2 = await _client.LoadProductAsync(created.Id);
                    Assert.Equal(newName, fetched2!.Name);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed during UPDATE step: " + ex.Message, ex);
                }

                // brisemo ga
                try
                {
                    var ok2 = await _client.DeleteProductAsync(created.Id);
                    Assert.True(ok2);

                    var fetched3 = await _client.LoadProductAsync(created.Id);
                    Assert.Null(fetched3);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed during DELETE step: " + ex.Message, ex);
                }
            }
            finally
            {
                if (created != null)
                {
                    try
                    {
                        await _client.DeleteProductAsync(created.Id); // safe cleanup
                    }
                    catch (Exception cleanupEx)
                    {
                        Console.WriteLine($"Cleanup failed: {cleanupEx.Message}");
                    }
                }
            }
        }

        [Fact]
        public async Task UpdateProductFails()
        {
            //izbacili smo required polja
            var badProduct = new ProductDto
            {
                Price = 99,
                CategoryId = 19,
                CategoryName = "Kave",
                ImageUrl = "update integracijski test"
                // Missing Name and Description
            };

            var response = await _client.UpdateProductAsync(badProduct);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task DeleteProductAsyncFails()
        {
            var result = await _client.DeleteProductAsync(-9999);
            Assert.False(result);
        }
    }

}