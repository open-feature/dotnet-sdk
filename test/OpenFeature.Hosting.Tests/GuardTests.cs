namespace OpenFeature.Hosting.Tests;

public class GuardTests
{
    [Fact]
    public void ThrowIfNull_WithNullArgument_ThrowsArgumentNullException()
    {
        // Arrange
        object? argument = null;

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => Guard.ThrowIfNull(argument));

        // Assert
        Assert.Equal("argument", exception.ParamName);
    }

    [Fact]
    public void ThrowIfNull_WithNotNullArgument_DoesNotThrowArgumentNullException()
    {
        // Arrange
        object? argument = "Test argument";

        // Act
        var ex = Record.Exception(() => Guard.ThrowIfNull(argument));

        // Assert
        Assert.Null(ex);
    }
}
