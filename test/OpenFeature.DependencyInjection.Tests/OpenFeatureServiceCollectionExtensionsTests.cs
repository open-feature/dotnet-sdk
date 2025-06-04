using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

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

        Assert.Single(_systemUnderTest, s => s.ServiceType == typeof(Api) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Single(_systemUnderTest, s => s.ServiceType == typeof(IFeatureLifecycleManager) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Single(_systemUnderTest, s => s.ServiceType == typeof(IFeatureClient) && s.Lifetime == ServiceLifetime.Scoped);
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
