using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using RestaurantMenuAPI.Data;
using RestaurantMenuAPI.Models;

namespace APPETIT_ASP.NET_SQL_SERVER.Tests;

public class DataSeedingTests : TestBase
{
    public DataSeedingTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task Database_Should_BeSeeded_WithRoles()
    {
        // Arrange
        using var scope = CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Act & Assert
        var guestRoleExists = await roleManager.RoleExistsAsync("Guest");
        var staffRoleExists = await roleManager.RoleExistsAsync("Staff");
        var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
        
        guestRoleExists.Should().BeTrue("Guest role should exist");
        staffRoleExists.Should().BeTrue("Staff role should exist");
        adminRoleExists.Should().BeTrue("Admin role should exist");
        
        Console.WriteLine("✅ Database roles seeding test passed");
    }

    [Fact]
    public async Task Database_Should_BeSeeded_WithAdminUser()
    {
        // Arrange
        using var scope = CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        // Act
        var adminUser = await userManager.FindByEmailAsync("admin@restaurant.com");
        
        // Assert
        adminUser.Should().NotBeNull("Admin user should exist");
        adminUser!.FirstName.Should().Be("System");
        adminUser.LastName.Should().Be("Administrator");
        adminUser.EmailConfirmed.Should().BeTrue("Admin user email should be confirmed");
        
        var isInAdminRole = await userManager.IsInRoleAsync(adminUser, "Admin");
        isInAdminRole.Should().BeTrue("Admin user should be in Admin role");
        
        Console.WriteLine("✅ Admin user seeding test passed");
    }

    [Fact]
    public async Task Database_Should_BeSeeded_WithMenuItems()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Act
        var menuItemsCount = await context.MenuItems.CountAsync();
        var appetizers = await context.MenuItems.Where(m => m.CategoryId == 1).CountAsync();
        var mainCourses = await context.MenuItems.Where(m => m.CategoryId == 2).CountAsync();
        var desserts = await context.MenuItems.Where(m => m.CategoryId == 3).CountAsync();
        var beverages = await context.MenuItems.Where(m => m.CategoryId == 4).CountAsync();
        
        // Assert
        menuItemsCount.Should().BeGreaterThan(0, "Menu items should be seeded");
        appetizers.Should().BeGreaterThan(0, "Appetizers should be seeded");
        mainCourses.Should().BeGreaterThan(0, "Main courses should be seeded");
        desserts.Should().BeGreaterThan(0, "Desserts should be seeded");
        beverages.Should().BeGreaterThan(0, "Beverages should be seeded");
        
        Console.WriteLine($"✅ Menu items seeding test passed - Total: {menuItemsCount} items");
    }

    [Fact]
    public async Task Seeded_MenuItems_Should_HaveCorrectData()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Act
        var caesarSalad = await context.MenuItems.FirstOrDefaultAsync(m => m.Name == "Caesar Salad");
        var ribeye = await context.MenuItems.FirstOrDefaultAsync(m => m.Name == "Ribeye Steak");
        var tiramisu = await context.MenuItems.FirstOrDefaultAsync(m => m.Name == "Tiramisu");
        
        // Assert
        caesarSalad.Should().NotBeNull("Caesar Salad should be seeded");
        caesarSalad!.Price.Should().Be(12.99m);
        caesarSalad.CategoryId.Should().Be(1);
        caesarSalad.IsAvailable.Should().BeTrue();
        
        ribeye.Should().NotBeNull("Ribeye Steak should be seeded");
        ribeye!.Price.Should().Be(32.99m);
        ribeye.AvailableQuantity.Should().Be(15);
        
        tiramisu.Should().NotBeNull("Tiramisu should be seeded");
        tiramisu!.Description.Should().Contain("Italian dessert");
        
        Console.WriteLine("✅ Seeded menu items data validation test passed");
    }
}