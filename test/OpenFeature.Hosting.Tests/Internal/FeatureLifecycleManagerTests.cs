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
        await lifecycleManager.EnsureInitializedAsync(TestContext.Current.CancellationToken);

        // Assert
        var actualProvider = api.GetProvider();
        Assert.Equal(provider, actualProvider);
    }

    [Fact]
    public async Task EnsureInitializedAsync_SetsMultipleProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider1 = new NoOpFeatureProvider();
        var provider2 = new NoOpFeatureProvider();
        services.AddOptions<OpenFeatureOptions>().Configure(options =>
        {
            options.AddProviderName("provider1");
            options.AddProviderName("provider2");
        });
        services.AddKeyedSingleton<FeatureProvider>("provider1", provider1);
        services.AddKeyedSingleton<FeatureProvider>("provider2", provider2);

        var api = Api.Instance;

        // Act
        using var serviceProvider = services.BuildServiceProvider();
        var lifecycleManager = new FeatureLifecycleManager(api, serviceProvider, NullLogger<FeatureLifecycleManager>.Instance);
        await lifecycleManager.EnsureInitializedAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(provider1, api.GetProvider("provider1"));
        Assert.Equal(provider2, api.GetProvider("provider2"));
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
        await lifecycleManager.EnsureInitializedAsync(TestContext.Current.CancellationToken);

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

        bool hookExecuted = false;
        services.AddSingleton(new EventHandlerDelegateWrapper(ProviderEventTypes.ProviderReady, (p) => { hookExecuted = true; }));

        var api = Api.Instance;

        // Act
        using var serviceProvider = services.BuildServiceProvider();
        var lifecycleManager = new FeatureLifecycleManager(api, serviceProvider, NullLogger<FeatureLifecycleManager>.Instance);
        await lifecycleManager.EnsureInitializedAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(hookExecuted);
    }

    [Fact]
    public async Task ShutdownAsync_ResetsApi()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = new NoOpFeatureProvider();

        var api = Api.Instance;
        await api.SetProviderAsync(provider, TestContext.Current.CancellationToken);
        api.AddHooks(new NoOpHook());

        // Act
        using var serviceProvider = services.BuildServiceProvider();
        var lifecycleManager = new FeatureLifecycleManager(api, serviceProvider, NullLogger<FeatureLifecycleManager>.Instance);
        await lifecycleManager.ShutdownAsync(TestContext.Current.CancellationToken);

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
        await lifecycleManager.EnsureInitializedAsync(TestContext.Current.CancellationToken);

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
        await lifecycleManager.ShutdownAsync(TestContext.Current.CancellationToken);

        // Assert
        var log = logger.LatestRecord;
        Assert.Equal(200, log.Id);
        Assert.Equal("Shutting down the feature provider", log.Message);
        Assert.Equal(LogLevel.Information, log.Level);
    }

    public async ValueTask InitializeAsync()
    {
        await Api.Instance.ShutdownAsync();
    }

    // Make sure the singleton is cleared between tests
    public async ValueTask DisposeAsync()
    {
        await Api.Instance.ShutdownAsync().ConfigureAwait(false);
    }
}
