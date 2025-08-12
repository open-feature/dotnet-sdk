using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Providers.MultiProvider.Tests.Strategies.Models;

public class ProviderErrorTests
{
    private const string TestProviderName = "test-provider";

    [Fact]
    public void Constructor_WithProviderNameAndException_CreatesProviderError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var providerError = new ProviderError(TestProviderName, exception);

        // Assert
        Assert.Equal(TestProviderName, providerError.ProviderName);
        Assert.Equal(exception, providerError.Error);
    }

    [Fact]
    public void Constructor_WithProviderNameAndNullException_CreatesProviderError()
    {
        // Act
        var providerError = new ProviderError(TestProviderName, null);

        // Assert
        Assert.Equal(TestProviderName, providerError.ProviderName);
        Assert.Null(providerError.Error);
    }

    [Fact]
    public void Constructor_WithNullProviderName_CreatesProviderError()
    {
        // Arrange
        var exception = new ArgumentException("Test exception");

        // Act
        var providerError = new ProviderError(null!, exception);

        // Assert
        Assert.Null(providerError.ProviderName);
        Assert.Equal(exception, providerError.Error);
    }

    [Fact]
    public void Constructor_WithEmptyProviderName_CreatesProviderError()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        var providerError = new ProviderError(string.Empty, exception);

        // Assert
        Assert.Equal(string.Empty, providerError.ProviderName);
        Assert.Equal(exception, providerError.Error);
    }

    [Fact]
    public void Constructor_WithWhitespaceProviderName_CreatesProviderError()
    {
        // Arrange
        const string whitespaceName = "   ";
        var exception = new NotSupportedException("Test exception");

        // Act
        var providerError = new ProviderError(whitespaceName, exception);

        // Assert
        Assert.Equal(whitespaceName, providerError.ProviderName);
        Assert.Equal(exception, providerError.Error);
    }

    [Fact]
    public void Constructor_WithDifferentExceptionTypes_CreatesProviderError()
    {
        // Arrange
        var argumentException = new ArgumentException("Argument exception");
        var invalidOperationException = new InvalidOperationException("Invalid operation exception");
        var notImplementedException = new NotImplementedException("Not implemented exception");

        // Act
        var providerError1 = new ProviderError("provider1", argumentException);
        var providerError2 = new ProviderError("provider2", invalidOperationException);
        var providerError3 = new ProviderError("provider3", notImplementedException);

        // Assert
        Assert.Equal("provider1", providerError1.ProviderName);
        Assert.Equal(argumentException, providerError1.Error);
        Assert.Equal("provider2", providerError2.ProviderName);
        Assert.Equal(invalidOperationException, providerError2.Error);
        Assert.Equal("provider3", providerError3.ProviderName);
        Assert.Equal(notImplementedException, providerError3.Error);
    }

    [Fact]
    public void ProviderName_Property_HasPrivateSetter()
    {
        // Act & Assert
        var providerNameProperty = typeof(ProviderError).GetProperty(nameof(ProviderError.ProviderName));
        Assert.NotNull(providerNameProperty);
        Assert.True(providerNameProperty.CanRead);
        Assert.True(providerNameProperty.CanWrite);
        Assert.True(providerNameProperty.GetSetMethod(true)?.IsPrivate);
    }

    [Fact]
    public void Error_Property_HasPrivateSetter()
    {
        // Act & Assert
        var errorProperty = typeof(ProviderError).GetProperty(nameof(ProviderError.Error));
        Assert.NotNull(errorProperty);
        Assert.True(errorProperty.CanRead);
        Assert.True(errorProperty.CanWrite);
        Assert.True(errorProperty.GetSetMethod(true)?.IsPrivate);
    }

    [Fact]
    public void Constructor_WithNullProviderNameAndNullException_CreatesProviderError()
    {
        // Act
        var providerError = new ProviderError(null!, null);

        // Assert
        Assert.Null(providerError.ProviderName);
        Assert.Null(providerError.Error);
    }

    [Fact]
    public void Constructor_WithLongProviderName_CreatesProviderError()
    {
        // Arrange
        var longProviderName = new string('a', 1000);
        var exception = new TimeoutException("Test exception");

        // Act
        var providerError = new ProviderError(longProviderName, exception);

        // Assert
        Assert.Equal(longProviderName, providerError.ProviderName);
        Assert.Equal(exception, providerError.Error);
    }
}
