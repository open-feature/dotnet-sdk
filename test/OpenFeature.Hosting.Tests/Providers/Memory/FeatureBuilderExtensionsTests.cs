using Microsoft.Extensions.DependencyInjection;
using OpenFeature.Hosting.Providers.Memory;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;

namespace OpenFeature.Hosting.Tests.Providers.Memory;

public class FeatureBuilderExtensionsTests
{
    [Fact]
    public void AddInMemoryProvider_AddsProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);

        // Act
        builder.AddInMemoryProvider();

        // Assert
        using var provider = services.BuildServiceProvider();

        var featureProvider = provider.GetService<FeatureProvider>();
        Assert.NotNull(featureProvider);
        Assert.IsType<InMemoryProvider>(featureProvider);
    }

    [Fact]
    public async Task AddInMemoryProvider_WithFlags_AddsProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);
        Func<EvaluationContext, string> enableForAlphaGroup = ctx => ctx.GetValue("group").AsString == "alpha" ? "on" : "off";
        var flags = new Dictionary<string, Flag>
        {
            { "feature1", new Flag<bool>(new Dictionary<string, bool> { { "on", true }, { "off", false } }, "on") },
            { "feature2", new Flag<bool>(new Dictionary<string, bool> { { "on", true }, { "off", false } }, "on", enableForAlphaGroup) }
        };

        // Act
        builder.AddInMemoryProvider((sp) => flags);

        // Assert
        using var provider = services.BuildServiceProvider();

        var featureProvider = provider.GetService<FeatureProvider>();
        Assert.NotNull(featureProvider);
        Assert.IsType<InMemoryProvider>(featureProvider);

        var result = await featureProvider.ResolveBooleanValueAsync("feature1", false);
        Assert.True(result.Value);
    }

    [Fact]
    public void AddInMemoryProvider_WithNullFlagsFactory_AddsProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);

        // Act
        builder.AddInMemoryProvider((sp) => null);

        // Assert
        using var provider = services.BuildServiceProvider();

        var featureProvider = provider.GetService<FeatureProvider>();
        Assert.NotNull(featureProvider);
        Assert.IsType<InMemoryProvider>(featureProvider);
    }

    [Fact]
    public void AddInMemoryProvider_WithNullConfigure_AddsProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);

        // Act
        builder.AddInMemoryProvider((Action<IDictionary<string, Flag>>?) null);

        // Assert
        using var provider = services.BuildServiceProvider();

        var featureProvider = provider.GetService<FeatureProvider>();
        Assert.NotNull(featureProvider);
        Assert.IsType<InMemoryProvider>(featureProvider);
    }

    [Fact]
    public void AddInMemoryProvider_WithDomain_AddsProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);

        // Act
        builder.AddInMemoryProvider("domain");

        // Assert
        using var provider = services.BuildServiceProvider();

        var featureProvider = provider.GetKeyedService<FeatureProvider>("domain");
        Assert.NotNull(featureProvider);
        Assert.IsType<InMemoryProvider>(featureProvider);
    }

    [Fact]
    public async Task AddInMemoryProvider_WithDomainAndFlags_AddsProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);
        Func<EvaluationContext, string> enableForAlphaGroup = ctx => ctx.GetValue("group").AsString == "alpha" ? "on" : "off";
        var flags = new Dictionary<string, Flag>
        {
            { "feature1", new Flag<bool>(new Dictionary<string, bool> { { "on", true }, { "off", false } }, "on") },
            { "feature2", new Flag<bool>(new Dictionary<string, bool> { { "on", true }, { "off", false } }, "off", enableForAlphaGroup) }
        };

        // Act
        builder.AddInMemoryProvider("domain", (sp) => flags);

        // Assert
        using var provider = services.BuildServiceProvider();

        var featureProvider = provider.GetKeyedService<FeatureProvider>("domain");
        Assert.NotNull(featureProvider);
        Assert.IsType<InMemoryProvider>(featureProvider);

        var context = EvaluationContext.Builder().Set("group", "alpha").Build();
        var result = await featureProvider.ResolveBooleanValueAsync("feature2", false, context);
        Assert.True(result.Value);
    }

    [Fact]
    public void AddInMemoryProvider_WithDomainAndNullFlagsFactory_AddsProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);

        // Act
        builder.AddInMemoryProvider("domain", (sp) => null);

        // Assert
        using var provider = services.BuildServiceProvider();

        var featureProvider = provider.GetKeyedService<FeatureProvider>("domain");
        Assert.NotNull(featureProvider);
        Assert.IsType<InMemoryProvider>(featureProvider);
    }

    [Fact]
    public async Task AddInMemoryProvider_WithOptions_AddsProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<InMemoryProviderOptions>()
            .Configure((opts) =>
            {
                opts.Flags = new Dictionary<string, Flag>
                {
                    { "new-feature", new Flag<bool>(new Dictionary<string, bool> { { "on", true }, { "off", false } }, "off") },
                };
            });

        var builder = new OpenFeatureBuilder(services);

        // Act
        builder.AddInMemoryProvider();

        // Assert
        using var provider = services.BuildServiceProvider();

        var featureProvider = provider.GetService<FeatureProvider>();
        Assert.NotNull(featureProvider);
        Assert.IsType<InMemoryProvider>(featureProvider);

        var result = await featureProvider.ResolveBooleanValueAsync("new-feature", true);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task AddInMemoryProvider_WithDomainAndOptions_AddsProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<InMemoryProviderOptions>("domain-name")
            .Configure((opts) =>
            {
                opts.Flags = new Dictionary<string, Flag>
                {
                    { "new-feature", new Flag<bool>(new Dictionary<string, bool> { { "on", true }, { "off", false } }, "off") },
                };
            });

        var builder = new OpenFeatureBuilder(services);

        // Act
        builder.AddInMemoryProvider("domain-name");

        // Assert
        using var provider = services.BuildServiceProvider();

        var featureProvider = provider.GetKeyedService<FeatureProvider>("domain-name");
        Assert.NotNull(featureProvider);
        Assert.IsType<InMemoryProvider>(featureProvider);

        var result = await featureProvider.ResolveBooleanValueAsync("new-feature", true);
        Assert.False(result.Value);
    }

    [Fact]
    public void AddInMemoryProvider_WithDomainAndNullOptions_AddsProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<InMemoryProviderOptions>("domain-name")
            .Configure((opts) =>
            {
                opts.Flags = null;
            });

        var builder = new OpenFeatureBuilder(services);

        // Act
        builder.AddInMemoryProvider("domain-name");

        // Assert
        using var provider = services.BuildServiceProvider();

        var featureProvider = provider.GetKeyedService<FeatureProvider>("domain-name");
        Assert.NotNull(featureProvider);
        Assert.IsType<InMemoryProvider>(featureProvider);
    }

    [Fact]
    public void AddInMemoryProvider_WithDomainAndNullConfigure_AddsProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        var builder = new OpenFeatureBuilder(services);

        // Act
        builder.AddInMemoryProvider("domain-name", (Action<IDictionary<string, Flag>>?) null);

        // Assert
        using var provider = services.BuildServiceProvider();

        var featureProvider = provider.GetKeyedService<FeatureProvider>("domain-name");
        Assert.NotNull(featureProvider);
        Assert.IsType<InMemoryProvider>(featureProvider);
    }
}
