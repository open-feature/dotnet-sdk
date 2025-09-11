using OpenFeature.Providers.MultiProvider.Models;

namespace OpenFeature.Providers.MultiProvider.Tests.Models;

public class ProviderStatusTests
{
    [Fact]
    public void Constructor_CreatesProviderStatusWithDefaultValues()
    {
        // Act
        var providerStatus = new ChildProviderStatus();

        // Assert
        Assert.Equal(string.Empty, providerStatus.ProviderName);
        Assert.Null(providerStatus.Error);
    }

    [Fact]
    public void ProviderName_CanBeSet()
    {
        // Arrange
        const string providerName = "test-provider";
        var providerStatus = new ChildProviderStatus();

        // Act
        providerStatus.ProviderName = providerName;

        // Assert
        Assert.Equal(providerName, providerStatus.ProviderName);
    }

    [Fact]
    public void ProviderName_CanBeSetToNull()
    {
        // Arrange
        var providerStatus = new ChildProviderStatus { ProviderName = "initial-name" };

        // Act
        providerStatus.ProviderName = null!;

        // Assert
        Assert.Null(providerStatus.ProviderName);
    }

    [Fact]
    public void ProviderName_CanBeSetToEmptyString()
    {
        // Arrange
        var providerStatus = new ChildProviderStatus { ProviderName = "initial-name" };

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
        var providerStatus = new ChildProviderStatus();

        // Act
        providerStatus.Error = exception;

        // Assert
        Assert.Equal(exception, providerStatus.Error);
    }

    [Fact]
    public void Exception_CanBeSetToNull()
    {
        // Arrange
        var providerStatus = new ChildProviderStatus { Error = new Exception("initial exception") };

        // Act
        providerStatus.Error = null;

        // Assert
        Assert.Null(providerStatus.Error);
    }

    [Fact]
    public void ProviderStatus_CanBeInitializedWithObjectInitializer()
    {
        // Arrange
        const string providerName = "test-provider";
        var exception = new ArgumentException("Test exception");

        // Act
        var providerStatus = new ChildProviderStatus
        {
            ProviderName = providerName,
            Error = exception
        };

        // Assert
        Assert.Equal(providerName, providerStatus.ProviderName);
        Assert.Equal(exception, providerStatus.Error);
    }

    [Fact]
    public void ProviderName_Property_HasGetterAndSetter()
    {
        // Act & Assert
        var providerNameProperty = typeof(ChildProviderStatus).GetProperty(nameof(ChildProviderStatus.ProviderName));
        Assert.NotNull(providerNameProperty);
        Assert.True(providerNameProperty.CanRead);
        Assert.True(providerNameProperty.CanWrite);
    }

    [Fact]
    public void Exception_Property_HasGetterAndSetter()
    {
        // Act & Assert
        var exceptionProperty = typeof(ChildProviderStatus).GetProperty(nameof(ChildProviderStatus.Error));
        Assert.NotNull(exceptionProperty);
        Assert.True(exceptionProperty.CanRead);
        Assert.True(exceptionProperty.CanWrite);
    }
}
