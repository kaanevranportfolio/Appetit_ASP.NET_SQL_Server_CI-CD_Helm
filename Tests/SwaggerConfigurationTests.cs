using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;
using FluentAssertions;
using System.Text.Json;

namespace APPETIT_ASP.NET_SQL_SERVER.Tests;

public class SwaggerConfigurationTests : TestBase
{
    public SwaggerConfigurationTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task Swagger_Json_Should_BeAccessible()
    {
        // Act
        var response = await Client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        Console.WriteLine("✅ Swagger JSON accessibility test passed");
    }

    [Fact]
    public async Task Swagger_Should_ContainApiInformation()
    {
        // Act
        var response = await Client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var swaggerDoc = JsonDocument.Parse(content);
        
        // Assert
        var info = swaggerDoc.RootElement.GetProperty("info");
        var title = info.GetProperty("title").GetString();
        var version = info.GetProperty("version").GetString();
        
        title.Should().Be("Restaurant Menu & Reservation API");
        version.Should().Be("v1");
        
        Console.WriteLine("✅ Swagger API information test passed");
    }

    [Fact]
    public async Task Swagger_Should_HaveBearerSecurity()
    {
        // Act
        var response = await Client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        
        // Assert
        content.Should().Contain("Bearer");
        content.Should().Contain("securitySchemes");
        content.Should().Contain("Authorization");
        
        Console.WriteLine("✅ Swagger Bearer security configuration test passed");
    }
}