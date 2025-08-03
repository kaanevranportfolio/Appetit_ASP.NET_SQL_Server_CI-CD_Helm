using Xunit;

namespace APPETIT_ASP.NET_SQL_SERVER.Tests;

public class BasicTest
{
    [Fact]
    public void Simple_Math_Test_Should_Pass()
    {
        // Arrange
        var a = 2;
        var b = 3;
        
        // Act
        var result = a + b;
        
        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void String_Test_Should_Pass()
    {
        // Arrange
        var text = "Hello World";
        
        // Act & Assert
        Assert.Contains("World", text);
        Assert.StartsWith("Hello", text);
    }
}