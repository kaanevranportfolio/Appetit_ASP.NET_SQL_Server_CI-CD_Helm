using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Xunit;
using FluentAssertions;
using RestaurantMenuAPI.Models;

namespace APPETIT_ASP.NET_SQL_SERVER.Tests;

public class IdentityTests : TestBase
{
    public IdentityTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public void Identity_Services_Should_BeRegistered()
    {
        // Arrange
        using var scope = CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        // Act & Assert
        var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
        var signInManager = serviceProvider.GetService<SignInManager<ApplicationUser>>();
        
        userManager.Should().NotBeNull("UserManager should be registered");
        roleManager.Should().NotBeNull("RoleManager should be registered");
        signInManager.Should().NotBeNull("SignInManager should be registered");
        
        Console.WriteLine("✅ Identity services registration test passed");
    }

    [Fact]
    public void Password_Policy_Should_BeConfigured()
    {
        // Arrange
        using var scope = CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        // Act
        var options = userManager.Options.Password;
        
        // Assert
        options.RequireDigit.Should().BeTrue("Password should require digit");
        options.RequireLowercase.Should().BeTrue("Password should require lowercase");
        options.RequireUppercase.Should().BeTrue("Password should require uppercase");
        options.RequireNonAlphanumeric.Should().BeFalse("Password should not require non-alphanumeric");
        options.RequiredLength.Should().Be(6, "Password should require minimum 6 characters");
        
        Console.WriteLine("✅ Password policy configuration test passed");
    }

    [Fact]
    public void User_Options_Should_BeConfigured()
    {
        // Arrange
        using var scope = CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        // Act
        var userOptions = userManager.Options.User;
        
        // Assert
        userOptions.RequireUniqueEmail.Should().BeTrue("User should require unique email");
        
        Console.WriteLine("✅ User options configuration test passed");
    }
}