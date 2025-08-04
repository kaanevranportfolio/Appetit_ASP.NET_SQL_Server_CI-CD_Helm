using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;
using FluentAssertions;
using System.Text.Json;

namespace RestaurantMenuAPI.Tests;

public class SwaggerConfigurationTests : TestBase
{
    public SwaggerConfigurationTests(WebApplicationFactory<Program> factory) : base(factory) { }


    private async Task<(HttpStatusCode status, string content)> GetSwaggerJsonWithRetry(int maxAttempts = 5, int delayMs = 1000)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var response = await Client.GetAsync("/swagger/v1/swagger.json");
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK && !string.IsNullOrWhiteSpace(content))
                return (response.StatusCode, content);
            await Task.Delay(delayMs);
        }
        // Final try
        var finalResponse = await Client.GetAsync("/swagger/v1/swagger.json");
        var finalContent = await finalResponse.Content.ReadAsStringAsync();
        return (finalResponse.StatusCode, finalContent);
    }

    [Fact]
    public async Task Swagger_Json_Should_BeAccessible()
    {
        // Act
        var (status, content) = await GetSwaggerJsonWithRetry();

        // Assert
        status.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNullOrEmpty();
        Console.WriteLine("✅ Swagger JSON accessibility test passed");
    }

    [Fact]
    public async Task Swagger_Should_ContainApiInformation()
    {
        // Act
        var (_, content) = await GetSwaggerJsonWithRetry();
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
        var (_, content) = await GetSwaggerJsonWithRetry();
        var swaggerDoc = JsonDocument.Parse(content);

        // Assert
        var root = swaggerDoc.RootElement;
        var components = root.GetProperty("components");
        var securitySchemes = components.GetProperty("securitySchemes");
        securitySchemes.TryGetProperty("Bearer", out var bearerScheme).Should().BeTrue("Bearer security scheme should be defined");
        bearerScheme.GetProperty("type").GetString().Should().Be("apiKey");
        bearerScheme.GetProperty("name").GetString().Should().Be("Authorization");
        bearerScheme.GetProperty("in").GetString().Should().Be("header");

        // Also check that security requirement references Bearer
        var security = root.GetProperty("security");
        bool foundBearer = false;
        foreach (var req in security.EnumerateArray())
        {
            foreach (var prop in req.EnumerateObject())
            {
                if (prop.Name == "Bearer")
                {
                    foundBearer = true;
                    break;
                }
            }
        }
        foundBearer.Should().BeTrue("Swagger security requirements should reference Bearer");

        Console.WriteLine("✅ Swagger Bearer security configuration test passed");
    }
}