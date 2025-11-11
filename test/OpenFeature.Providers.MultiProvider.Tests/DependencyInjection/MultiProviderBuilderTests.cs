using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.DependencyInjection;
using OpenFeature.Providers.MultiProvider.Strategies;
using OpenFeature.Providers.MultiProvider.Tests.Utils;

namespace OpenFeature.Providers.MultiProvider.Tests.DependencyInjection;

public class MultiProviderBuilderTests
{
    private readonly FeatureProvider _mockProvider = Substitute.For<FeatureProvider>();

    public MultiProviderBuilderTests()
    {
        _mockProvider.GetMetadata().Returns(new Metadata("mock-provider"));
    }

    [Fact]
    public void AddProvider_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new MultiProviderBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.AddProvider(null!, _mockProvider));
    }

    [Fact]
    public void AddProvider_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new MultiProviderBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.AddProvider("", _mockProvider));
    }

    [Fact]
    public void AddProvider_WithNullProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new MultiProviderBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.AddProvider("test", (FeatureProvider)null!));
    }

    [Fact]
    public void AddProvider_WithFactory_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new MultiProviderBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.AddProvider(null!, sp => _mockProvider));
    }

    [Fact]
    public void AddProvider_WithFactory_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new MultiProviderBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.AddProvider("test", (Func<IServiceProvider, FeatureProvider>)null!));
    }

    [Fact]
    public void AddProvider_Generic_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new MultiProviderBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.AddProvider<TestProvider>(null!));
    }

    [Fact]
    public void AddProvider_Generic_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new MultiProviderBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.AddProvider<TestProvider>(""));
    }

    [Fact]
    public void AddProvider_AddsProviderToBuilder()
    {
        // Arrange
        var builder = new MultiProviderBuilder();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        builder.AddProvider("test-provider", _mockProvider);
        var entries = builder.BuildProviderEntries(serviceProvider);

        // Assert
        Assert.Single(entries);
        Assert.Equal("test-provider", entries[0].Name);
        Assert.Same(_mockProvider, entries[0].Provider);
    }

    [Fact]
    public void AddProvider_WithFactory_AddsProviderToBuilder()
    {
        // Arrange
        var builder = new MultiProviderBuilder();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        builder.AddProvider("test-provider", sp => _mockProvider);
        var entries = builder.BuildProviderEntries(serviceProvider);

        // Assert
        Assert.Single(entries);
        Assert.Equal("test-provider", entries[0].Name);
        Assert.Same(_mockProvider, entries[0].Provider);
    }

    [Fact]
    public void AddProvider_Generic_WithFactory_AddsProviderToBuilder()
    {
        // Arrange
        var builder = new MultiProviderBuilder();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        builder.AddProvider("test-provider", sp => new TestProvider("test-provider"));
        var entries = builder.BuildProviderEntries(serviceProvider);

        // Assert
        Assert.Single(entries);
        Assert.Equal("test-provider", entries[0].Name);
        Assert.IsType<TestProvider>(entries[0].Provider);
    }

    [Fact]
    public void AddProvider_Generic_WithoutFactory_ResolvesFromServiceProvider()
    {
        // Arrange
        var builder = new MultiProviderBuilder();
        var services = new ServiceCollection();
        services.AddTransient(_ => new TestProvider("test-provider"));
        var serviceProvider = services.BuildServiceProvider();

        // Act
        builder.AddProvider<TestProvider>("test-provider");
        var entries = builder.BuildProviderEntries(serviceProvider);

        // Assert
        Assert.Single(entries);
        Assert.Equal("test-provider", entries[0].Name);
        Assert.IsType<TestProvider>(entries[0].Provider);
    }

    [Fact]
    public void AddProvider_MultipleProviders_AddsAllProviders()
    {
        // Arrange
        var builder = new MultiProviderBuilder();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var provider3 = Substitute.For<FeatureProvider>();

        provider1.GetMetadata().Returns(new Metadata("provider1"));
        provider2.GetMetadata().Returns(new Metadata("provider2"));
        provider3.GetMetadata().Returns(new Metadata("provider3"));

        // Act
        builder
            .AddProvider("provider1", provider1)
            .AddProvider("provider2", sp => provider2)
            .AddProvider("provider3", sp => new TestProvider("provider3"));

        var entries = builder.BuildProviderEntries(serviceProvider);

        // Assert
        Assert.Equal(3, entries.Count);
        Assert.Equal("provider1", entries[0].Name);
        Assert.Equal("provider2", entries[1].Name);
        Assert.Equal("provider3", entries[2].Name);
    }

    [Fact]
    public void UseStrategy_Generic_SetsStrategy()
    {
        // Arrange
        var builder = new MultiProviderBuilder();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        builder.UseStrategy<FirstMatchStrategy>();
        var strategy = builder.BuildEvaluationStrategy(serviceProvider);

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<FirstMatchStrategy>(strategy);
    }

    [Fact]
    public void UseStrategy_WithInstance_SetsStrategy()
    {
        // Arrange
        var builder = new MultiProviderBuilder();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var strategyInstance = new FirstMatchStrategy();

        // Act
        builder.UseStrategy(strategyInstance);
        var strategy = builder.BuildEvaluationStrategy(serviceProvider);

        // Assert
        Assert.NotNull(strategy);
        Assert.Same(strategyInstance, strategy);
    }

    [Fact]
    public void UseStrategy_WithNullInstance_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new MultiProviderBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.UseStrategy((FirstMatchStrategy)null!));
    }

    [Fact]
    public void UseStrategy_WithFactory_SetsStrategy()
    {
        // Arrange
        var builder = new MultiProviderBuilder();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        builder.UseStrategy(sp => new FirstMatchStrategy());
        var strategy = builder.BuildEvaluationStrategy(serviceProvider);

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<FirstMatchStrategy>(strategy);
    }

    [Fact]
    public void UseStrategy_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new MultiProviderBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.UseStrategy((Func<IServiceProvider, FirstMatchStrategy>)null!));
    }

    [Fact]
    public void BuildEvaluationStrategy_WithNoStrategy_ReturnsNull()
    {
        // Arrange
        var builder = new MultiProviderBuilder();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var strategy = builder.BuildEvaluationStrategy(serviceProvider);

        // Assert
        Assert.Null(strategy);
    }

    [Fact]
    public void BuildProviderEntries_WithNoProviders_ReturnsEmptyList()
    {
        // Arrange
        var builder = new MultiProviderBuilder();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var entries = builder.BuildProviderEntries(serviceProvider);

        // Assert
        Assert.NotNull(entries);
        Assert.Empty(entries);
    }

    [Fact]
    public void Builder_ChainsMethodsCorrectly()
    {
        // Arrange
        var builder = new MultiProviderBuilder();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = builder
            .AddProvider("provider1", _mockProvider)
            .AddProvider("provider2", sp => _mockProvider)
            .UseStrategy<FirstMatchStrategy>();

        var entries = builder.BuildProviderEntries(serviceProvider);
        var strategy = builder.BuildEvaluationStrategy(serviceProvider);

        // Assert
        Assert.Same(builder, result);
        Assert.Equal(2, entries.Count);
        Assert.NotNull(strategy);
    }
}
