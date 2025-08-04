using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Xunit;
using FluentAssertions;

namespace RestaurantMenuAPI.Tests;

public class JwtConfigurationTests : TestBase
{
    public JwtConfigurationTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public void JWT_Configuration_Should_BeValid()
    {
        // Arrange
        using var scope = CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        // Act
        var secretKey = config["JwtSettings:SecretKey"];
        var issuer = config["JwtSettings:Issuer"];
        var audience = config["JwtSettings:Audience"];
        
        // Assert
        secretKey.Should().NotBeNullOrEmpty("JWT SecretKey should be configured");
        secretKey!.Length.Should().BeGreaterThan(32, "JWT SecretKey should be sufficiently long");
        issuer.Should().NotBeNullOrEmpty("JWT Issuer should be configured");
        audience.Should().NotBeNullOrEmpty("JWT Audience should be configured");
        
        Console.WriteLine($"✅ JWT configuration test passed - Issuer: {issuer}, Audience: {audience}");
    }

    [Fact]
    public void JWT_Bearer_Options_Should_BeConfigured()
    {
        // Arrange
        using var scope = CreateScope();
        var jwtOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
        
        // Act
        var options = jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        
        // Assert
        options.Should().NotBeNull("JWT Bearer options should be configured");
        options.SaveToken.Should().BeTrue("JWT should save token");
        options.RequireHttpsMetadata.Should().BeFalse("HTTPS not required in test environment");
        
        Console.WriteLine("✅ JWT Bearer options configuration test passed");
    }
}