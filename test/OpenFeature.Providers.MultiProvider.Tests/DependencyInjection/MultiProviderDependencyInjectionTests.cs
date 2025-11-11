using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OpenFeature.Hosting;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.DependencyInjection;
using OpenFeature.Providers.MultiProvider.Strategies;
using OpenFeature.Providers.MultiProvider.Tests.Utils;

namespace OpenFeature.Providers.MultiProvider.Tests.DependencyInjection;

public class MultiProviderDependencyInjectionTests
{
    private readonly FeatureProvider _mockProvider = Substitute.For<FeatureProvider>();

    public MultiProviderDependencyInjectionTests()
    {
        _mockProvider.GetMetadata().Returns(new Metadata("test-provider"));
        _mockProvider.ResolveBooleanValueAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ResolutionDetails<bool>("test", true)));
    }

    [Fact]
    public void AddMultiProvider_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        OpenFeatureBuilder builder = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.AddMultiProvider(b => b.AddProvider("test", _mockProvider)));
    }

    [Fact]
    public void AddMultiProvider_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.AddMultiProvider(null!));
    }

    [Fact]
    public void AddMultiProvider_WithDomain_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        OpenFeatureBuilder builder = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.AddMultiProvider("test-domain", b => b.AddProvider("test", _mockProvider)));
    }

    [Fact]
    public void AddMultiProvider_WithDomain_WithNullDomain_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.AddMultiProvider(null!, b => b.AddProvider("test", _mockProvider)));
    }

    [Fact]
    public void AddMultiProvider_WithDomain_WithEmptyDomain_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.AddMultiProvider("", b => b.AddProvider("test", _mockProvider)));
    }

    [Fact]
    public void AddMultiProvider_WithDomain_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.AddMultiProvider("test-domain", null!));
    }

    [Fact]
    public void AddMultiProvider_WithNoProviders_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOpenFeature(builder =>
        {
            builder.AddMultiProvider(b => { }); // Empty configuration
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<FeatureProvider>());
    }

    [Fact]
    public void AddMultiProvider_RegistersProviderCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOpenFeature(builder =>
        {
            builder.AddMultiProvider(b =>
            {
                b.AddProvider("provider1", _mockProvider);
            });
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var provider = serviceProvider.GetRequiredService<FeatureProvider>();

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<MultiProvider>(provider);
    }

    [Fact]
    public void AddMultiProvider_WithDomain_RegistersProviderCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOpenFeature(builder =>
        {
            builder.AddMultiProvider("test-domain", b =>
            {
                b.AddProvider("provider1", _mockProvider);
            });
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var provider = serviceProvider.GetRequiredKeyedService<FeatureProvider>("test-domain");

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<MultiProvider>(provider);
    }

    [Fact]
    public void AddMultiProvider_WithMultipleProviders_CreatesMultiProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var provider3 = Substitute.For<FeatureProvider>();

        provider1.GetMetadata().Returns(new Metadata("provider1"));
        provider2.GetMetadata().Returns(new Metadata("provider2"));
        provider3.GetMetadata().Returns(new Metadata("provider3"));

        services.AddOpenFeature(builder =>
        {
            builder.AddMultiProvider(b =>
            {
                b.AddProvider("provider1", provider1)
                 .AddProvider("provider2", provider2)
                 .AddProvider("provider3", provider3);
            });
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var provider = serviceProvider.GetRequiredService<FeatureProvider>();

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<MultiProvider>(provider);
    }

    [Fact]
    public void AddMultiProvider_WithStrategy_CreatesMultiProviderWithStrategy()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOpenFeature(builder =>
        {
            builder.AddMultiProvider(b =>
            {
                b.AddProvider("provider1", _mockProvider)
                 .UseStrategy<FirstMatchStrategy>();
            });
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var provider = serviceProvider.GetRequiredService<FeatureProvider>();

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<MultiProvider>(provider);
    }

    [Fact]
    public void AddMultiProvider_WithFactoryProvider_CreatesProviderFromFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var factoryCalled = false;

        services.AddOpenFeature(builder =>
        {
            builder.AddMultiProvider(b =>
            {
                b.AddProvider("provider1", sp =>
                {
                    factoryCalled = true;
                    var mockProvider = Substitute.For<FeatureProvider>();
                    mockProvider.GetMetadata().Returns(new Metadata("factory-provider"));
                    return mockProvider;
                });
            });
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var provider = serviceProvider.GetRequiredService<FeatureProvider>();

        // Assert
        Assert.NotNull(provider);
        Assert.True(factoryCalled, "Factory method should have been called");
    }

    [Fact]
    public void AddMultiProvider_WithTypedProvider_ResolvesFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient(_ => new TestProvider("test-provider"));

        services.AddOpenFeature(builder =>
        {
            builder.AddMultiProvider(b =>
            {
                b.AddProvider<TestProvider>("provider1");
            });
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var provider = serviceProvider.GetRequiredService<FeatureProvider>();

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<MultiProvider>(provider);
    }

    [Fact]
    public void AddMultiProvider_WithLogger_CreatesMultiProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddOpenFeature(builder =>
        {
            builder.AddMultiProvider(b =>
            {
                b.AddProvider("provider1", _mockProvider);
            });
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var provider = serviceProvider.GetRequiredService<FeatureProvider>();

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<MultiProvider>(provider);
    }
}
