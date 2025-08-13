using NSubstitute;
using OpenFeature.Providers.Memory;
using OpenFeature.Providers.MultiProvider.Models;

namespace OpenFeature.Providers.MultiProvider.Tests.Models;

public class RegisteredProviderTests
{
    private readonly FeatureProvider _mockProvider = Substitute.For<FeatureProvider>();
    private const string TestProviderName = "test-provider";

    [Fact]
    public void Constructor_WithValidParameters_CreatesRegisteredProvider()
    {
        // Act
        var registeredProvider = new RegisteredProvider(this._mockProvider, TestProviderName);

        // Assert
        Assert.Equal(this._mockProvider, registeredProvider.Provider);
        Assert.Equal(TestProviderName, registeredProvider.Name);
    }

    [Fact]
    public void Constructor_WithNullProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new RegisteredProvider(null!, TestProviderName));
        Assert.Equal("provider", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new RegisteredProvider(this._mockProvider, null!));
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyName_CreatesRegisteredProviderWithEmptyName()
    {
        // Act
        var registeredProvider = new RegisteredProvider(this._mockProvider, string.Empty);

        // Assert
        Assert.Equal(this._mockProvider, registeredProvider.Provider);
        Assert.Equal(string.Empty, registeredProvider.Name);
    }

    [Fact]
    public void Constructor_WithWhitespaceName_CreatesRegisteredProviderWithWhitespaceName()
    {
        // Arrange
        const string whitespaceName = "   ";

        // Act
        var registeredProvider = new RegisteredProvider(this._mockProvider, whitespaceName);

        // Assert
        Assert.Equal(this._mockProvider, registeredProvider.Provider);
        Assert.Equal(whitespaceName, registeredProvider.Name);
    }

    [Fact]
    public void Constructor_WithSameProviderAndDifferentNames_CreatesDistinctInstances()
    {
        // Arrange
        const string name1 = "provider-1";
        const string name2 = "provider-2";

        // Act
        var registeredProvider1 = new RegisteredProvider(this._mockProvider, name1);
        var registeredProvider2 = new RegisteredProvider(this._mockProvider, name2);

        // Assert
        Assert.Equal(this._mockProvider, registeredProvider1.Provider);
        Assert.Equal(this._mockProvider, registeredProvider2.Provider);
        Assert.Equal(name1, registeredProvider1.Name);
        Assert.Equal(name2, registeredProvider2.Name);
        Assert.NotEqual(registeredProvider1.Name, registeredProvider2.Name);
    }

    [Fact]
    public void Constructor_WithDifferentProvidersAndSameName_CreatesDistinctInstances()
    {
        // Arrange
        var mockProvider2 = Substitute.For<FeatureProvider>();

        // Act
        var registeredProvider1 = new RegisteredProvider(this._mockProvider, TestProviderName);
        var registeredProvider2 = new RegisteredProvider(mockProvider2, TestProviderName);

        // Assert
        Assert.Equal(this._mockProvider, registeredProvider1.Provider);
        Assert.Equal(mockProvider2, registeredProvider2.Provider);
        Assert.Equal(TestProviderName, registeredProvider1.Name);
        Assert.Equal(TestProviderName, registeredProvider2.Name);
        Assert.NotEqual(registeredProvider1.Provider, registeredProvider2.Provider);
    }

    [Theory]
    [InlineData(Constant.ProviderStatus.Ready)]
    [InlineData(Constant.ProviderStatus.Error)]
    [InlineData(Constant.ProviderStatus.Fatal)]
    [InlineData(Constant.ProviderStatus.NotReady)]
    public void SetStatus_WithDifferentStatuses_UpdatesCorrectly(Constant.ProviderStatus status)
    {
        // Arrange
        var registeredProvider = new RegisteredProvider(new InMemoryProvider(), "test");

        // Act
        registeredProvider.SetStatus(status);

        // Assert
        Assert.Equal(status, registeredProvider.Status);
    }
}
