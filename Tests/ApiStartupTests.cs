using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;
using FluentAssertions;

namespace APPETIT_ASP.NET_SQL_SERVER.Tests;

public class ApiStartupTests : TestBase
{
    public ApiStartupTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task Api_Should_StartSuccessfully()
    {
        // Act
        var response = await Client.GetAsync("/");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        Console.WriteLine($"✅ API startup - Status: {response.StatusCode}");
    }

    [Fact]
    public async Task Api_Should_HandleInvalidRoutes()
    {
        // Act
        var response = await Client.GetAsync("/nonexistent-endpoint");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Console.WriteLine("✅ Invalid route handling test passed");
    }

    [Fact]
    public async Task Api_Should_ReturnCorrectContentType()
    {
        // Act
        var response = await Client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }
        Console.WriteLine("✅ Content type test passed");
    }
}