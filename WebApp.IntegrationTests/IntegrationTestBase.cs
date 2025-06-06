using System.Net.Http;
using Microsoft.Extensions.Logging;
using WebApp.ApiClients;

public abstract class IntegrationTestBase
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger<CategoriesApiClient> Logger;

    protected IntegrationTestBase()
    {
        HttpClient = new HttpClient
        {
            BaseAddress = new Uri("https://oicar-team-11-ajugostitelj-11.onrender.com/api/") // 
        };

        Logger = new LoggerFactory().CreateLogger<CategoriesApiClient>();
    }
}
