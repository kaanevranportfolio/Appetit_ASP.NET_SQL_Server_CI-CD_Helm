using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using RestaurantMenuAPI.Data;

namespace RestaurantMenuAPI.Tests;

public class DatabaseTests : TestBase
{
    public DatabaseTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public void Database_Context_Should_BeConfigured()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Act & Assert
        context.Should().NotBeNull();
        context.Database.Should().NotBeNull();
        
        var connectionString = context.Database.GetConnectionString();
        connectionString.Should().Contain("RestaurantMenuTestDB");
        
        Console.WriteLine("✅ Database context configuration test passed");
    }

    [Fact]
    public async Task Database_Should_BeAccessible()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Act & Assert
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue("Database should be accessible");
        
        Console.WriteLine("✅ Database accessibility test passed");
    }

    [Fact]
    public async Task Database_Should_HaveCorrectTables()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Act
        var hasMenuItems = await context.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) as Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MenuItems'"
        ).FirstOrDefaultAsync();
        
        var hasUsers = await context.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) as Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers'"
        ).FirstOrDefaultAsync();
        
        // Assert
        hasMenuItems.Should().Be(1, "MenuItems table should exist");
        hasUsers.Should().Be(1, "AspNetUsers table should exist");
        
        Console.WriteLine("✅ Database tables existence test passed");
    }
}