using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenFeature.Internal;
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
        _mockServiceProvider.GetService(typeof(FeatureProvider))
            .Returns(featureProvider);

        // Act
        await _systemUnderTest.EnsureInitializedAsync().ConfigureAwait(true);

        // Assert
        Api.Instance.GetProvider().Should().BeSameAs(featureProvider);
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
        exception.Message.Should().Be("Feature provider is not registered in the service collection.");
    }
}
