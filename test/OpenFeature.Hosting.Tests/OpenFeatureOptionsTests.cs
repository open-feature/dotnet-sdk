namespace OpenFeature.Hosting.Tests;

public class OpenFeatureOptionsTests
{
    [Fact]
    public void AddProviderName_DoesNotSetHasDefaultProvider()
    {
        // Arrange
        var options = new OpenFeatureOptions();

        // Act
        options.AddProviderName("TestProvider");

        // Assert
        Assert.False(options.HasDefaultProvider);
    }

    [Fact]
    public void AddProviderName_WithNullName_SetsHasDefaultProvider()
    {
        // Arrange
        var options = new OpenFeatureOptions();

        // Act
        options.AddProviderName(null);

        // Assert
        Assert.True(options.HasDefaultProvider);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddProviderName_WithEmptyName_SetsHasDefaultProvider(string name)
    {
        // Arrange
        var options = new OpenFeatureOptions();

        // Act
        options.AddProviderName(name);

        // Assert
        Assert.True(options.HasDefaultProvider);
    }

    [Fact]
    public void AddProviderName_WithSameName_OnlyRegistersNameOnce()
    {
        // Arrange
        var options = new OpenFeatureOptions();

        // Act
        options.AddProviderName("test-provider");
        options.AddProviderName("test-provider");
        options.AddProviderName("test-provider");

        // Assert
        Assert.Single(options.ProviderNames);
    }

    [Fact]
    public void AddHookName_RegistersHookName()
    {
        // Arrange
        var options = new OpenFeatureOptions();

        // Act
        options.AddHookName("test-hook");

        // Assert
        Assert.Single(options.HookNames);
    }
}
