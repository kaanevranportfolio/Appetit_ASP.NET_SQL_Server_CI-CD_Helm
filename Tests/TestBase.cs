using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace APPETIT_ASP.NET_SQL_SERVER.Tests;

public abstract class TestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;

    protected TestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Override configuration for testing
                config.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", 
                        "Server=sqlserver-test;Database=RestaurantMenuTestDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"),
                    new KeyValuePair<string, string?>("JwtSettings:SecretKey", "TestSecretKey123456789012345678901234567890"),
                    new KeyValuePair<string, string?>("JwtSettings:Issuer", "TestRestaurantAPI"),
                    new KeyValuePair<string, string?>("JwtSettings:Audience", "TestRestaurantUsers"),
                    new KeyValuePair<string, string?>("Serilog:MinimumLevel", "Warning")
                });
            });
        });
        
        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();
}