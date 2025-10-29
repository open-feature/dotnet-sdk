using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace OpenFeature.Hosting.Tests;

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

    [Fact]
    public void AddOpenFeature_WithDefaultProvider()
    {
        // Act
        _systemUnderTest.AddOpenFeature(builder =>
        {
            builder.AddProvider(_ => new NoOpFeatureProvider());
        });

        // Assert
        using var serviceProvider = _systemUnderTest.BuildServiceProvider();
        var featureClient = serviceProvider.GetRequiredService<IFeatureClient>();
        Assert.NotNull(featureClient);
    }

    [Fact]
    public void AddOpenFeature_WithNamedDefaultProvider()
    {
        // Act
        _systemUnderTest.AddOpenFeature(builder =>
        {
            builder.AddProvider("no-opprovider", (_, key) => new NoOpFeatureProvider());
        });

        // Assert
        using var serviceProvider = _systemUnderTest.BuildServiceProvider();
        var featureClient = serviceProvider.GetRequiredService<IFeatureClient>();
        Assert.NotNull(featureClient);
    }

    [Fact]
    public void AddOpenFeature_WithNamedDefaultProvider_InvokesAddPolicyName()
    {
        // Arrange
        var provider1 = new NoOpFeatureProvider();
        var provider2 = new NoOpFeatureProvider();

        // Act
        _systemUnderTest.AddOpenFeature(builder =>
        {
            builder
                .AddPolicyName(ss =>
                {
                    ss.DefaultNameSelector = (sp) => "no-opprovider";
                })
                .AddProvider("no-opprovider", (_, key) => provider1)
                .AddProvider("no-opprovider-2", (_, key) => provider2);
        });

        // Assert
        using var serviceProvider = _systemUnderTest.BuildServiceProvider();
        var client = serviceProvider.GetKeyedService<IFeatureClient>("no-opprovider");
        Assert.NotNull(client);

        var otherClient = serviceProvider.GetService<IFeatureClient>();
        Assert.NotNull(otherClient);
    }

    [Fact]
    public void AddOpenFeature_WithNoProvider_CanResolveFeatureClient()
    {
        // Act
        _systemUnderTest.AddOpenFeature(builder => {});

        // Assert
        using var serviceProvider = _systemUnderTest.BuildServiceProvider();
        var client = serviceProvider.GetService<IFeatureClient>();
        Assert.NotNull(client);
    }
}
