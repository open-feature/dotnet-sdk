using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Isolated;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;

namespace OpenFeature.Tests.Isolated;

/// <summary>
/// Tests for isolated API instances (spec section 1.8).
/// Each test creates its own isolated instances and cleans them up,
/// so no shared state fixture is needed.
/// </summary>
#if NET8_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.Experimental(FeatureDiagnosticCodes.IsolatedApi)]
#endif
public class IsolatedApiTests
{
    [Fact]
    [Specification("1.8.1", "The API MUST expose a factory function which creates and returns a new, independent instance of the API.")]
    public void CreateIsolated_Should_Return_New_Instance()
    {
        var isolated = OpenFeatureFactory.CreateIsolated();

        Assert.NotNull(isolated);
        Assert.NotSame(Api.Instance, isolated);
    }

    [Fact]
    [Specification("1.8.1", "The API MUST expose a factory function which creates and returns a new, independent instance of the API.")]
    public void CreateIsolated_Should_Return_Distinct_Instances()
    {
        var isolated1 = OpenFeatureFactory.CreateIsolated();
        var isolated2 = OpenFeatureFactory.CreateIsolated();

        Assert.NotSame(isolated1, isolated2);
    }

    [Fact]
    [Specification("1.8.2", "Instances returned by the factory function MUST conform to the same API contract as the global singleton.")]
    public async Task Isolated_Instance_Should_Support_Full_Api_Contract()
    {
        var isolated = OpenFeatureFactory.CreateIsolated();
        try
        {
            // Provider management
            var provider = new TestProvider();
            await isolated.SetProviderAsync(provider);
            Assert.Equal(provider, isolated.GetProvider());
            Assert.Equal(provider.GetMetadata()?.Name, isolated.GetProviderMetadata()?.Name);

            // Client creation
            var client = isolated.GetClient("test-client");
            Assert.NotNull(client);
            Assert.Equal("test-client", client.GetMetadata().Name);

            // Hooks
            var hook = Substitute.For<Hook>();
            isolated.AddHooks(hook);
            Assert.Contains(hook, isolated.GetHooks());

            // Context
            var context = EvaluationContext.Builder().Set("key", "value").Build();
            isolated.SetContext(context);
            Assert.Equal(context, isolated.GetContext());

            // Event handling (just verify it doesn't throw)
            isolated.AddHandler(ProviderEventTypes.ProviderReady, _ => { });

            // Transaction context
            var propagator = Substitute.For<ITransactionContextPropagator>();
            isolated.SetTransactionContextPropagator(propagator);
        }
        finally
        {
            await isolated.ShutdownAsync();
        }
    }

    [Fact]
    [Specification("1.8.1", "The API MUST expose a factory function which creates and returns a new, independent instance of the API.")]
    public async Task Isolated_Provider_Should_Not_Affect_Singleton()
    {
        Api.ResetApi();
        var isolated = OpenFeatureFactory.CreateIsolated();
        try
        {
            var provider = new TestProvider("isolated-provider");
            await isolated.SetProviderAsync(provider);

            // The singleton should still have the default NoOp provider
            Assert.Equal(NoOpProvider.NoOpProviderName, Api.Instance.GetProviderMetadata()?.Name);
            Assert.Equal("isolated-provider", isolated.GetProviderMetadata()?.Name);
        }
        finally
        {
            await isolated.ShutdownAsync();
        }
    }

    [Fact]
    [Specification("1.8.1", "The API MUST expose a factory function which creates and returns a new, independent instance of the API.")]
    public async Task Isolated_Provider_Should_Not_Affect_Other_Isolated_Instance()
    {
        var isolated1 = OpenFeatureFactory.CreateIsolated();
        var isolated2 = OpenFeatureFactory.CreateIsolated();
        try
        {
            var provider1 = new TestProvider("provider-1");
            var provider2 = new TestProvider("provider-2");
            await isolated1.SetProviderAsync(provider1);
            await isolated2.SetProviderAsync(provider2);

            Assert.Equal("provider-1", isolated1.GetProviderMetadata()?.Name);
            Assert.Equal("provider-2", isolated2.GetProviderMetadata()?.Name);
        }
        finally
        {
            await isolated1.ShutdownAsync();
            await isolated2.ShutdownAsync();
        }
    }

    [Fact]
    [Specification("1.8.1", "The API MUST expose a factory function which creates and returns a new, independent instance of the API.")]
    public void Isolated_Hooks_Should_Not_Affect_Other_Instances()
    {
        var isolated1 = OpenFeatureFactory.CreateIsolated();
        var isolated2 = OpenFeatureFactory.CreateIsolated();

        var hook1 = Substitute.For<Hook>();
        var hook2 = Substitute.For<Hook>();

        isolated1.AddHooks(hook1);
        isolated2.AddHooks(hook2);

        Assert.Contains(hook1, isolated1.GetHooks());
        Assert.DoesNotContain(hook2, isolated1.GetHooks());

        Assert.Contains(hook2, isolated2.GetHooks());
        Assert.DoesNotContain(hook1, isolated2.GetHooks());
    }

    [Fact]
    [Specification("1.8.1", "The API MUST expose a factory function which creates and returns a new, independent instance of the API.")]
    public void Isolated_Context_Should_Not_Affect_Other_Instances()
    {
        var isolated1 = OpenFeatureFactory.CreateIsolated();
        var isolated2 = OpenFeatureFactory.CreateIsolated();

        var context1 = EvaluationContext.Builder().Set("instance", "one").Build();
        var context2 = EvaluationContext.Builder().Set("instance", "two").Build();

        isolated1.SetContext(context1);
        isolated2.SetContext(context2);

        Assert.Equal(context1, isolated1.GetContext());
        Assert.Equal(context2, isolated2.GetContext());
    }

    [Fact]
    [Specification("1.8.4", "A provider instance SHOULD NOT be registered with more than one API instance simultaneously.")]
    public async Task Same_Provider_Should_Throw_When_Bound_To_Multiple_Api_Instances()
    {
        var isolated1 = OpenFeatureFactory.CreateIsolated();
        var isolated2 = OpenFeatureFactory.CreateIsolated();
        try
        {
            var provider = new TestProvider("shared-provider");
            await isolated1.SetProviderAsync(provider);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => isolated2.SetProviderAsync(provider));

            Assert.Equal("This provider instance is already bound to a different API instance. " +
                "A provider should not be registered with more than one API instance simultaneously.", exception.Message);
        }
        finally
        {
            await isolated1.ShutdownAsync();
            await isolated2.ShutdownAsync();
        }
    }

    [Fact]
    [Specification("1.8.4", "A provider instance SHOULD NOT be registered with more than one API instance simultaneously.")]
    public async Task Same_Provider_Should_Throw_When_Bound_To_Multiple_Api_Instances_Domain()
    {
        var isolated1 = OpenFeatureFactory.CreateIsolated();
        var isolated2 = OpenFeatureFactory.CreateIsolated();
        try
        {
            var provider = new TestProvider("shared-provider");
            await isolated1.SetProviderAsync("domain-1", provider);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => isolated2.SetProviderAsync("domain-2", provider));

            Assert.Equal("This provider instance is already bound to a different API instance. " +
                "A provider should not be registered with more than one API instance simultaneously.", exception.Message);
        }
        finally
        {
            await isolated1.ShutdownAsync();
            await isolated2.ShutdownAsync();
        }
    }

    [Fact]
    [Specification("1.8.4", "A provider instance SHOULD NOT be registered with more than one API instance simultaneously.")]
    public async Task Concurrent_Binding_Should_Allow_Only_One_Api_Instance()
    {
        const int instanceCount = 10;
        var provider = new TestProvider("contested-provider");
        var instances = Enumerable.Range(0, instanceCount).Select(_ => OpenFeatureFactory.CreateIsolated()).ToList();
        try
        {
            var tasks = instances.Select(api => api.SetProviderAsync(provider)).ToList();
            var results = await Task.WhenAll(tasks.Select(async t =>
            {
                try { await t; return (Exception?)null; }
                catch (InvalidOperationException ex) { return ex; }
            }));

            var successes = results.Count(r => r is null);
            var failures = results.Count(r => r is InvalidOperationException);

            Assert.Equal(1, successes);
            Assert.Equal(instanceCount - 1, failures);
        }
        finally
        {
            foreach (var api in instances)
            {
                await api.ShutdownAsync();
            }
        }
    }

    [Fact]
    [Specification("1.8.4", "A provider instance SHOULD NOT be registered with more than one API instance simultaneously.")]
    public async Task Provider_Can_Be_Rebound_After_Shutdown()
    {
        var isolated1 = OpenFeatureFactory.CreateIsolated();
        var isolated2 = OpenFeatureFactory.CreateIsolated();
        try
        {
            var provider = new TestProvider("rebindable-provider");
            await isolated1.SetProviderAsync(provider);

            // Shut down first instance — this frees the provider
            await isolated1.ShutdownAsync();

            // Now the provider should be bindable to another instance
            await isolated2.SetProviderAsync(provider);
            Assert.Equal("rebindable-provider", isolated2.GetProviderMetadata()?.Name);
        }
        finally
        {
            await isolated2.ShutdownAsync();
        }
    }

    [Fact]
    [Specification("1.8.4", "A provider instance SHOULD NOT be registered with more than one API instance simultaneously.")]
    public async Task Same_Provider_Can_Be_Bound_To_Multiple_Domains_Within_Same_Api()
    {
        var isolated = OpenFeatureFactory.CreateIsolated();
        try
        {
            var provider = new TestProvider("multi-domain-provider");
            await isolated.SetProviderAsync("domain-a", provider);
            await isolated.SetProviderAsync("domain-b", provider);

            Assert.Equal(provider, isolated.GetProvider("domain-a"));
            Assert.Equal(provider, isolated.GetProvider("domain-b"));
        }
        finally
        {
            await isolated.ShutdownAsync();
        }
    }

    [Fact]
    public async Task Shutdown_Isolated_Should_Not_Affect_Singleton()
    {
        Api.ResetApi();
        var singletonProvider = new TestProvider("singleton-provider");
        await Api.Instance.SetProviderAsync(singletonProvider);

        var isolated = OpenFeatureFactory.CreateIsolated();
        var isolatedProvider = new TestProvider("isolated-provider");
        await isolated.SetProviderAsync(isolatedProvider);

        // Shutting down isolated instance
        await isolated.ShutdownAsync();

        // Singleton should be unaffected
        Assert.Equal("singleton-provider", Api.Instance.GetProviderMetadata()?.Name);

        // Cleanup
        await Api.Instance.ShutdownAsync();
    }

    [Fact]
    public async Task Shutdown_Isolated_Should_Not_Affect_Other_Isolated()
    {
        var isolated1 = OpenFeatureFactory.CreateIsolated();
        var isolated2 = OpenFeatureFactory.CreateIsolated();
        try
        {
            var provider1 = new TestProvider("provider-1");
            var provider2 = new TestProvider("provider-2");
            await isolated1.SetProviderAsync(provider1);
            await isolated2.SetProviderAsync(provider2);

            await isolated1.ShutdownAsync();

            // isolated2 should be unaffected
            Assert.Equal("provider-2", isolated2.GetProviderMetadata()?.Name);
        }
        finally
        {
            await isolated2.ShutdownAsync();
        }
    }

    [Fact]
    public async Task Provider_Can_Be_Rebound_After_Replacement()
    {
        var isolated1 = OpenFeatureFactory.CreateIsolated();
        var isolated2 = OpenFeatureFactory.CreateIsolated();
        try
        {
            var providerA = new TestProvider("provider-a");
            var providerB = new TestProvider("provider-b");

            await isolated1.SetProviderAsync(providerA);

            // Replace providerA with providerB in isolated1 — providerA should be unbound
            await isolated1.SetProviderAsync(providerB);

            // Allow some time for the async shutdown to complete
            await Task.Delay(100);

            // providerA should now be free to bind to isolated2
            await isolated2.SetProviderAsync(providerA);
            Assert.Equal("provider-a", isolated2.GetProviderMetadata()?.Name);
        }
        finally
        {
            await isolated1.ShutdownAsync();
            await isolated2.ShutdownAsync();
        }
    }

    [Fact]
    [Specification("1.8.2", "Instances returned by the factory function MUST conform to the same API contract as the global singleton.")]
    public async Task Isolated_Client_Should_Evaluate_Flags()
    {
        var isolated = OpenFeatureFactory.CreateIsolated();
        try
        {
            var provider = new TestProvider("eval-provider");
            await isolated.SetProviderAsync(provider);

            var client = isolated.GetClient();

            // TestProvider inverts boolean defaults
            var result = await client.GetBooleanValueAsync("test-flag", false);
            Assert.True(result);
        }
        finally
        {
            await isolated.ShutdownAsync();
        }
    }
}
