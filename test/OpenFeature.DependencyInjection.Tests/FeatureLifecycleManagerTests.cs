using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using OpenFeature.DependencyInjection.Internal;
using Xunit;

namespace OpenFeature.DependencyInjection.Tests;

public class FeatureLifecycleManagerTests
{
    private readonly FeatureLifecycleManager _systemUnderTest;
    private readonly IServiceProvider _mockServiceProvider;

    public FeatureLifecycleManagerTests()
    {
        Api.Instance.SetContext(null);
        Api.Instance.ClearHooks();

        _mockServiceProvider = Substitute.For<IServiceProvider>();

        var options = new OpenFeatureOptions();
        options.AddDefaultProviderName();
        var optionsMock = Substitute.For<IOptions<OpenFeatureOptions>>();
        optionsMock.Value.Returns(options);

        _mockServiceProvider.GetService<IOptions<OpenFeatureOptions>>().Returns(optionsMock);

        _systemUnderTest = new FeatureLifecycleManager(
            Api.Instance,
            _mockServiceProvider,
            Substitute.For<ILogger<FeatureLifecycleManager>>());
    }

    [Fact]
    public async Task EnsureInitializedAsync_ShouldLogAndSetProvider_WhenProviderExists()
    {
        // Arrange
        var featureProvider = new NoOpFeatureProvider();
        _mockServiceProvider.GetService(typeof(FeatureProvider)).Returns(featureProvider);

        // Act
        await _systemUnderTest.EnsureInitializedAsync().ConfigureAwait(true);

        // Assert
        Assert.Equal(featureProvider, Api.Instance.GetProvider());
    }

    [Fact]
    public async Task EnsureInitializedAsync_ShouldThrowException_WhenProviderDoesNotExist()
    {
        // Arrange
        _mockServiceProvider.GetService(typeof(FeatureProvider)).Returns(null as FeatureProvider);

        // Act
        var act = () => _systemUnderTest.EnsureInitializedAsync().AsTask();

        // Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act).ConfigureAwait(true);
        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
        Assert.False(string.IsNullOrWhiteSpace(exception.Message));
    }
}
