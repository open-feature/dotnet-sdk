using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace OpenFeature.DependencyInjection.Tests;

public class OpenFeatureServiceCollectionExtensionsTests
{
    private readonly IServiceCollection _systemUnderTest;
    private readonly Action<OpenFeatureBuilder> _configureAction;

    public OpenFeatureServiceCollectionExtensionsTests()
    {
        _systemUnderTest = new ServiceCollection();
        _configureAction = Substitute.For<Action<OpenFeatureBuilder>>();
    }

    [Fact]
    public void AddOpenFeature_ShouldRegisterApiInstanceAndLifecycleManagerAsSingleton()
    {
        // Act
        _systemUnderTest.AddOpenFeature(_configureAction);

        _systemUnderTest.Should().ContainSingle(s => s.ServiceType == typeof(Api) && s.Lifetime == ServiceLifetime.Singleton);
        _systemUnderTest.Should().ContainSingle(s => s.ServiceType == typeof(IFeatureLifecycleManager) && s.Lifetime == ServiceLifetime.Singleton);
        _systemUnderTest.Should().ContainSingle(s => s.ServiceType == typeof(IFeatureClient) && s.Lifetime == ServiceLifetime.Scoped);
        _systemUnderTest.Should().ContainSingle(s => s.ServiceType == typeof(FeatureProvider) && s.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void AddOpenFeature_ShouldInvokeConfigureAction()
    {
        // Act
        _systemUnderTest.AddOpenFeature(_configureAction);

        // Assert
        _configureAction.Received(1).Invoke(Arg.Any<OpenFeatureBuilder>());
    }
}
