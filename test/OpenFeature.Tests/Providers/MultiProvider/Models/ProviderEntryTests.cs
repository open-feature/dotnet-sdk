using NSubstitute;
using OpenFeature.Providers.MultiProvider.Models;

namespace OpenFeature.Tests.Providers.MultiProvider.Models;

public class ProviderEntryTests
{
    private readonly FeatureProvider _mockProvider = Substitute.For<FeatureProvider>();

    [Fact]
    public void Constructor_WithProvider_CreatesProviderEntry()
    {
        // Act
        var providerEntry = new ProviderEntry(this._mockProvider);

        // Assert
        Assert.Equal(this._mockProvider, providerEntry.Provider);
        Assert.Null(providerEntry.Name);
    }

    [Fact]
    public void Constructor_WithProviderAndName_CreatesProviderEntry()
    {
        // Arrange
        const string customName = "custom-provider-name";

        // Act
        var providerEntry = new ProviderEntry(this._mockProvider, customName);

        // Assert
        Assert.Equal(this._mockProvider, providerEntry.Provider);
        Assert.Equal(customName, providerEntry.Name);
    }

    [Fact]
    public void Constructor_WithNullProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ProviderEntry(null!));
        Assert.Equal("provider", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullName_CreatesProviderEntryWithNullName()
    {
        // Act
        var providerEntry = new ProviderEntry(this._mockProvider, null);

        // Assert
        Assert.Equal(this._mockProvider, providerEntry.Provider);
        Assert.Null(providerEntry.Name);
    }

    [Fact]
    public void Constructor_WithEmptyName_CreatesProviderEntryWithEmptyName()
    {
        // Act
        var providerEntry = new ProviderEntry(this._mockProvider, string.Empty);

        // Assert
        Assert.Equal(this._mockProvider, providerEntry.Provider);
        Assert.Equal(string.Empty, providerEntry.Name);
    }

    [Fact]
    public void Provider_Property_IsReadOnly()
    {
        // Arrange
        var providerEntry = new ProviderEntry(this._mockProvider);

        // Act & Assert
        // Verify that Provider property is read-only by checking it has no setter
        var providerProperty = typeof(ProviderEntry).GetProperty(nameof(ProviderEntry.Provider));
        Assert.NotNull(providerProperty);
        Assert.True(providerProperty.CanRead);
        Assert.False(providerProperty.CanWrite);
    }

    [Fact]
    public void Name_Property_IsReadOnly()
    {
        // Arrange
        const string customName = "test-name";
        var providerEntry = new ProviderEntry(this._mockProvider, customName);

        // Act & Assert
        // Verify that Name property is read-only by checking it has no setter
        var nameProperty = typeof(ProviderEntry).GetProperty(nameof(ProviderEntry.Name));
        Assert.NotNull(nameProperty);
        Assert.True(nameProperty.CanRead);
        Assert.False(nameProperty.CanWrite);
    }
}
