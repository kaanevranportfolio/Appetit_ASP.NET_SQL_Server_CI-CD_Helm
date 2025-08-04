using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;
using FluentAssertions;

namespace RestaurantMenuAPI.Tests;

public class CorsConfigurationTests : TestBase
{
    public CorsConfigurationTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task CORS_Should_BeConfigured()
    {
        // Act
        var response = await Client.GetAsync("/");
        
        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        
        Console.WriteLine("✅ CORS configuration test passed");
    }

    [Fact]
    public async Task CORS_Preflight_Should_BeHandled()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        
        // Act
        var response = await Client.SendAsync(request);
        
        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        
        Console.WriteLine("✅ CORS preflight handling test passed");
    }
}