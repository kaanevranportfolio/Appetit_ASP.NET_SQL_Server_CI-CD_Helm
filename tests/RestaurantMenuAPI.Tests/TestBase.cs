using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using Xunit;
using System.Threading.Tasks;
using RestaurantMenuAPI.Models;

namespace RestaurantMenuAPI.Tests;

public abstract class TestBase : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
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
                    new KeyValuePair<string, string?>("JwtSettings:SecretKey", "TestSecretKey123456789012345678901234567890"),
                    new KeyValuePair<string, string?>("JwtSettings:Issuer", "TestRestaurantAPI"),
                    new KeyValuePair<string, string?>("JwtSettings:Audience", "TestRestaurantUsers"),
                    new KeyValuePair<string, string?>("Serilog:MinimumLevel", "Warning")
                });
            });
            builder.ConfigureServices(services =>
            {
                // Remove the existing ApplicationDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RestaurantMenuAPI.Data.ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                // Register InMemory database for tests, use a unique name per test run
                services.AddDbContext<RestaurantMenuAPI.Data.ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                });
            });
        });

        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    // Reset the database before each test
    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantMenuAPI.Data.ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<RestaurantMenuAPI.Models.ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        // Call seeding methods from Program
        await Program.SeedRolesAsync(roleManager);
        await Program.SeedAdminUserAsync(userManager);
        await Program.SeedMenuItemsAsync(db);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}