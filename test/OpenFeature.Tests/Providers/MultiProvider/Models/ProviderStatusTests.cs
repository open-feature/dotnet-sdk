using OpenFeature.Providers.MultiProvider.Models;

namespace OpenFeature.Tests.Providers.MultiProvider.Models;

public class ProviderStatusTests
{
    [Fact]
    public void Constructor_CreatesProviderStatusWithDefaultValues()
    {
        // Act
        var providerStatus = new ProviderStatus();

        // Assert
        Assert.Equal(string.Empty, providerStatus.ProviderName);
        Assert.Null(providerStatus.Exception);
    }

    [Fact]
    public void ProviderName_CanBeSet()
    {
        // Arrange
        const string providerName = "test-provider";
        var providerStatus = new ProviderStatus();

        // Act
        providerStatus.ProviderName = providerName;

        // Assert
        Assert.Equal(providerName, providerStatus.ProviderName);
    }

    [Fact]
    public void ProviderName_CanBeSetToNull()
    {
        // Arrange
        var providerStatus = new ProviderStatus { ProviderName = "initial-name" };

        // Act
        providerStatus.ProviderName = null!;

        // Assert
        Assert.Null(providerStatus.ProviderName);
    }

    [Fact]
    public void ProviderName_CanBeSetToEmptyString()
    {
        // Arrange
        var providerStatus = new ProviderStatus { ProviderName = "initial-name" };

        // Act
        providerStatus.ProviderName = string.Empty;

        // Assert
        Assert.Equal(string.Empty, providerStatus.ProviderName);
    }

    [Fact]
    public void Exception_CanBeSet()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        var providerStatus = new ProviderStatus();

        // Act
        providerStatus.Exception = exception;

        // Assert
        Assert.Equal(exception, providerStatus.Exception);
    }

    [Fact]
    public void Exception_CanBeSetToNull()
    {
        // Arrange
        var providerStatus = new ProviderStatus { Exception = new Exception("initial exception") };

        // Act
        providerStatus.Exception = null;

        // Assert
        Assert.Null(providerStatus.Exception);
    }

    [Fact]
    public void ProviderStatus_CanBeInitializedWithObjectInitializer()
    {
        // Arrange
        const string providerName = "test-provider";
        var exception = new ArgumentException("Test exception");

        // Act
        var providerStatus = new ProviderStatus
        {
            ProviderName = providerName,
            Exception = exception
        };

        // Assert
        Assert.Equal(providerName, providerStatus.ProviderName);
        Assert.Equal(exception, providerStatus.Exception);
    }

    [Fact]
    public void ProviderName_Property_HasGetterAndSetter()
    {
        // Act & Assert
        var providerNameProperty = typeof(ProviderStatus).GetProperty(nameof(ProviderStatus.ProviderName));
        Assert.NotNull(providerNameProperty);
        Assert.True(providerNameProperty.CanRead);
        Assert.True(providerNameProperty.CanWrite);
    }

    [Fact]
    public void Exception_Property_HasGetterAndSetter()
    {
        // Act & Assert
        var exceptionProperty = typeof(ProviderStatus).GetProperty(nameof(ProviderStatus.Exception));
        Assert.NotNull(exceptionProperty);
        Assert.True(exceptionProperty.CanRead);
        Assert.True(exceptionProperty.CanWrite);
    }
}
