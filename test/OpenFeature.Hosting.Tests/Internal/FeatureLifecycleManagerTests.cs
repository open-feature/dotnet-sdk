using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using OpenFeature.Constant;
using OpenFeature.Hosting.Internal;

namespace OpenFeature.Hosting.Tests.Internal;

public class FeatureLifecycleManagerTests : IAsyncLifetime
{
    [Fact]
    public async Task EnsureInitializedAsync_SetsProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = new NoOpFeatureProvider();
        services.AddOptions<OpenFeatureOptions>().Configure(options =>
        {
            options.AddProviderName(null);
        });
        services.AddSingleton<FeatureProvider>(provider);

        var api = Api.Instance;

        // Act
        using var serviceProvider = services.BuildServiceProvider();
        var lifecycleManager = new FeatureLifecycleManager(api, serviceProvider, NullLogger<FeatureLifecycleManager>.Instance);
        await lifecycleManager.EnsureInitializedAsync();

        // Assert
        var actualProvider = api.GetProvider();
        Assert.Equal(provider, actualProvider);
    }

    [Fact]
    public async Task EnsureInitializedAsync_AddsHooks()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = new NoOpFeatureProvider();
        var hook = new NoOpHook();
        services.AddOptions<OpenFeatureOptions>().Configure(options =>
        {
            options.AddProviderName(null);
            options.AddHookName("TestHook");
        });
        services.AddSingleton<FeatureProvider>(provider);
        services.AddKeyedSingleton<Hook>("TestHook", hook);

        var api = Api.Instance;

        // Act
        using var serviceProvider = services.BuildServiceProvider();
        var lifecycleManager = new FeatureLifecycleManager(api, serviceProvider, NullLogger<FeatureLifecycleManager>.Instance);
        await lifecycleManager.EnsureInitializedAsync();

        // Assert
        var actualHooks = api.GetHooks();
        Assert.Single(actualHooks);
        Assert.Contains(hook, actualHooks);
    }

    [Fact]
    public async Task EnsureInitializedAsync_AddHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = new NoOpFeatureProvider();
        services.AddOptions<OpenFeatureOptions>().Configure(options =>
        {
            options.AddProviderName(null);
        });
        services.AddSingleton<FeatureProvider>(provider);

        string? message = null;
        services.AddSingleton(new EventHandlerDelegateWrapper(ProviderEventTypes.ProviderReady, (p) => { message = p?.Message; }));

        var api = Api.Instance;

        // Act
        using var serviceProvider = services.BuildServiceProvider();
        var lifecycleManager = new FeatureLifecycleManager(api, serviceProvider, NullLogger<FeatureLifecycleManager>.Instance);
        await lifecycleManager.EnsureInitializedAsync();

        // Assert
        Assert.NotNull(message);
        Assert.Equal("Provider is ready", message);
    }

    [Fact]
    public async Task ShutdownAsync_ResetsApi()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = new NoOpFeatureProvider();

        var api = Api.Instance;
        await api.SetProviderAsync(provider);
        api.AddHooks(new NoOpHook());

        // Act
        using var serviceProvider = services.BuildServiceProvider();
        var lifecycleManager = new FeatureLifecycleManager(api, serviceProvider, NullLogger<FeatureLifecycleManager>.Instance);
        await lifecycleManager.ShutdownAsync();

        // Assert
        var actualProvider = api.GetProvider();
        Assert.NotEqual(provider, actualProvider); // Default provider should be set after shutdown
        Assert.Empty(api.GetHooks()); // Hooks should be cleared
    }

    [Fact]
    public async Task EnsureInitializedAsync_LogStartingInitialization()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = new NoOpFeatureProvider();
        services.AddOptions<OpenFeatureOptions>().Configure(options =>
        {
            options.AddProviderName(null);
        });
        services.AddSingleton<FeatureProvider>(provider);

        var api = Api.Instance;
        var logger = new FakeLogger<FeatureLifecycleManager>();

        // Act
        using var serviceProvider = services.BuildServiceProvider();
        var lifecycleManager = new FeatureLifecycleManager(api, serviceProvider, logger);
        await lifecycleManager.EnsureInitializedAsync();

        // Assert
        var log = logger.LatestRecord;
        Assert.Equal(200, log.Id);
        Assert.Equal("Starting initialization of the feature provider", log.Message);
        Assert.Equal(LogLevel.Information, log.Level);
    }

    [Fact]
    public async Task ShutdownAsync_LogShuttingDown()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = new NoOpFeatureProvider();
        services.AddOptions<OpenFeatureOptions>().Configure(options =>
        {
            options.AddProviderName(null);
        });
        services.AddSingleton<FeatureProvider>(provider);

        var api = Api.Instance;
        var logger = new FakeLogger<FeatureLifecycleManager>();

        // Act
        using var serviceProvider = services.BuildServiceProvider();
        var lifecycleManager = new FeatureLifecycleManager(api, serviceProvider, logger);
        await lifecycleManager.ShutdownAsync();

        // Assert
        var log = logger.LatestRecord;
        Assert.Equal(200, log.Id);
        Assert.Equal("Shutting down the feature provider", log.Message);
        Assert.Equal(LogLevel.Information, log.Level);
    }

    public async Task InitializeAsync()
    {
        await Api.Instance.ShutdownAsync();
    }

    // Make sure the singleton is cleared between tests
    public async Task DisposeAsync()
    {
        await Api.Instance.ShutdownAsync().ConfigureAwait(false);
    }
}
