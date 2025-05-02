using System;
using System.Threading.Tasks;
using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;
using Xunit;

// We intentionally do not await for purposes of validating behavior.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace OpenFeature.Tests;

public class ProviderRepositoryTests
{
    [Fact]
    public async Task Default_Provider_Is_Set_Without_Await()
    {
        var repository = new ProviderRepository();
        var provider = new NoOpFeatureProvider();
        var context = new EvaluationContextBuilder().Build();
        await repository.SetProviderAsync(provider, context);
        Assert.Equal(provider, repository.GetProvider());
    }

    [Fact]
    public async Task Initialization_Provider_Method_Is_Invoked_For_Setting_Default_Provider()
    {
        var repository = new ProviderRepository();
        var providerMock = Substitute.For<FeatureProvider>();
        providerMock.Status.Returns(ProviderStatus.NotReady);
        var context = new EvaluationContextBuilder().Build();
        await repository.SetProviderAsync(providerMock, context);
        providerMock.Received(1).InitializeAsync(context);
        providerMock.DidNotReceive().ShutdownAsync();
    }

    [Fact]
    public async Task AfterInitialization_Is_Invoked_For_Setting_Default_Provider()
    {
        var repository = new ProviderRepository();
        var providerMock = Substitute.For<FeatureProvider>();
        providerMock.Status.Returns(ProviderStatus.NotReady);
        var context = new EvaluationContextBuilder().Build();
        var callCount = 0;
        await repository.SetProviderAsync(providerMock, context, afterInitSuccess: (theProvider) =>
        {
            Assert.Equal(providerMock, theProvider);
            callCount++;
            return Task.CompletedTask;
        });
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task AfterError_Is_Invoked_If_Initialization_Errors_Default_Provider()
    {
        var repository = new ProviderRepository();
        var providerMock = Substitute.For<FeatureProvider>();
        providerMock.Status.Returns(ProviderStatus.NotReady);
        var context = new EvaluationContextBuilder().Build();
        providerMock.When(x => x.InitializeAsync(context)).Throw(new Exception("BAD THINGS"));
        var callCount = 0;
        Exception? receivedError = null;
        await repository.SetProviderAsync(providerMock, context, afterInitError: (theProvider, error) =>
        {
            Assert.Equal(providerMock, theProvider);
            callCount++;
            receivedError = error;
            return Task.CompletedTask;
        });
        Assert.Equal("BAD THINGS", receivedError?.Message);
        Assert.Equal(1, callCount);
    }

    [Theory]
    [InlineData(ProviderStatus.Ready)]
    [InlineData(ProviderStatus.Stale)]
    [InlineData(ProviderStatus.Error)]
    internal async Task Initialize_Is_Not_Called_For_Ready_Provider(ProviderStatus status)
    {
        var repository = new ProviderRepository();
        var providerMock = Substitute.For<FeatureProvider>();
        providerMock.Status.Returns(status);
        var context = new EvaluationContextBuilder().Build();
        await repository.SetProviderAsync(providerMock, context);
        providerMock.DidNotReceive().InitializeAsync(context);
    }

    [Theory]
    [InlineData(ProviderStatus.Ready)]
    [InlineData(ProviderStatus.Stale)]
    [InlineData(ProviderStatus.Error)]
    internal async Task AfterInitialize_Is_Not_Called_For_Ready_Provider(ProviderStatus status)
    {
        var repository = new ProviderRepository();
        var providerMock = Substitute.For<FeatureProvider>();
        providerMock.Status.Returns(status);
        var context = new EvaluationContextBuilder().Build();
        var callCount = 0;
        await repository.SetProviderAsync(providerMock, context, afterInitSuccess: provider =>
        {
            callCount++;
            return Task.CompletedTask;
        });
        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task Replaced_Default_Provider_Is_Shutdown()
    {
        var repository = new ProviderRepository();
        var provider1 = Substitute.For<FeatureProvider>();
        provider1.Status.Returns(ProviderStatus.NotReady);

        var provider2 = Substitute.For<FeatureProvider>();
        provider2.Status.Returns(ProviderStatus.NotReady);

        var context = new EvaluationContextBuilder().Build();
        await repository.SetProviderAsync(provider1, context);
        await repository.SetProviderAsync(provider2, context);
        provider1.Received(1).ShutdownAsync();
        provider2.DidNotReceive().ShutdownAsync();
    }

    [Fact]
    public async Task Named_Provider_Provider_Is_Set_Without_Await()
    {
        var repository = new ProviderRepository();
        var provider = new NoOpFeatureProvider();
        var context = new EvaluationContextBuilder().Build();

        await repository.SetProviderAsync("the-name", provider, context);
        Assert.Equal(provider, repository.GetProvider("the-name"));
    }

    [Fact]
    public async Task Initialization_Provider_Method_Is_Invoked_For_Setting_Named_Provider()
    {
        var repository = new ProviderRepository();
        var providerMock = Substitute.For<FeatureProvider>();
        providerMock.Status.Returns(ProviderStatus.NotReady);
        var context = new EvaluationContextBuilder().Build();
        await repository.SetProviderAsync("the-name", providerMock, context);
        providerMock.Received(1).InitializeAsync(context);
        providerMock.DidNotReceive().ShutdownAsync();
    }

    [Fact]
    public async Task AfterInitialization_Is_Invoked_For_Setting_Named_Provider()
    {
        var repository = new ProviderRepository();
        var providerMock = Substitute.For<FeatureProvider>();
        providerMock.Status.Returns(ProviderStatus.NotReady);
        var context = new EvaluationContextBuilder().Build();
        var callCount = 0;
        await repository.SetProviderAsync("the-name", providerMock, context, afterInitSuccess: (theProvider) =>
        {
            Assert.Equal(providerMock, theProvider);
            callCount++;
            return Task.CompletedTask;
        });
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task AfterError_Is_Invoked_If_Initialization_Errors_Named_Provider()
    {
        var repository = new ProviderRepository();
        var providerMock = Substitute.For<FeatureProvider>();
        providerMock.Status.Returns(ProviderStatus.NotReady);
        var context = new EvaluationContextBuilder().Build();
        providerMock.When(x => x.InitializeAsync(context)).Throw(new Exception("BAD THINGS"));
        var callCount = 0;
        Exception? receivedError = null;
        await repository.SetProviderAsync("the-provider", providerMock, context, afterInitError: (theProvider, error) =>
        {
            Assert.Equal(providerMock, theProvider);
            callCount++;
            receivedError = error;
            return Task.CompletedTask;
        });
        Assert.Equal("BAD THINGS", receivedError?.Message);
        Assert.Equal(1, callCount);
    }

    [Theory]
    [InlineData(ProviderStatus.Ready)]
    [InlineData(ProviderStatus.Stale)]
    [InlineData(ProviderStatus.Error)]
    internal async Task Initialize_Is_Not_Called_For_Ready_Named_Provider(ProviderStatus status)
    {
        var repository = new ProviderRepository();
        var providerMock = Substitute.For<FeatureProvider>();
        providerMock.Status.Returns(status);
        var context = new EvaluationContextBuilder().Build();
        await repository.SetProviderAsync("the-name", providerMock, context);
        providerMock.DidNotReceive().InitializeAsync(context);
    }

    [Theory]
    [InlineData(ProviderStatus.Ready)]
    [InlineData(ProviderStatus.Stale)]
    [InlineData(ProviderStatus.Error)]
    internal async Task AfterInitialize_Is_Not_Called_For_Ready_Named_Provider(ProviderStatus status)
    {
        var repository = new ProviderRepository();
        var providerMock = Substitute.For<FeatureProvider>();
        providerMock.Status.Returns(status);
        var context = new EvaluationContextBuilder().Build();
        var callCount = 0;
        await repository.SetProviderAsync("the-name", providerMock, context,
            afterInitSuccess: provider =>
            {
                callCount++;
                return Task.CompletedTask;
            });
        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task Replaced_Named_Provider_Is_Shutdown()
    {
        var repository = new ProviderRepository();
        var provider1 = Substitute.For<FeatureProvider>();
        provider1.Status.Returns(ProviderStatus.NotReady);

        var provider2 = Substitute.For<FeatureProvider>();
        provider2.Status.Returns(ProviderStatus.NotReady);

        var context = new EvaluationContextBuilder().Build();
        await repository.SetProviderAsync("the-name", provider1, context);
        await repository.SetProviderAsync("the-name", provider2, context);
        provider1.Received(1).ShutdownAsync();
        provider2.DidNotReceive().ShutdownAsync();
    }

    [Fact]
    public async Task In_Use_Provider_Named_And_Default_Is_Not_Shutdown()
    {
        var repository = new ProviderRepository();
        var provider1 = Substitute.For<FeatureProvider>();
        provider1.Status.Returns(ProviderStatus.NotReady);

        var provider2 = Substitute.For<FeatureProvider>();
        provider2.Status.Returns(ProviderStatus.NotReady);

        var context = new EvaluationContextBuilder().Build();

        await repository.SetProviderAsync(provider1, context);
        await repository.SetProviderAsync("A", provider1, context);
        // Provider one is replaced for "A", but not default.
        await repository.SetProviderAsync("A", provider2, context);

        provider1.DidNotReceive().ShutdownAsync();
    }

    [Fact]
    public async Task In_Use_Provider_Two_Named_Is_Not_Shutdown()
    {
        var repository = new ProviderRepository();
        var provider1 = Substitute.For<FeatureProvider>();
        provider1.Status.Returns(ProviderStatus.NotReady);

        var provider2 = Substitute.For<FeatureProvider>();
        provider2.Status.Returns(ProviderStatus.NotReady);

        var context = new EvaluationContextBuilder().Build();

        await repository.SetProviderAsync("B", provider1, context);
        await repository.SetProviderAsync("A", provider1, context);
        // Provider one is replaced for "A", but not "B".
        await repository.SetProviderAsync("A", provider2, context);

        provider1.DidNotReceive().ShutdownAsync();
    }

    [Fact]
    public async Task When_All_Instances_Are_Removed_Shutdown_Is_Called()
    {
        var repository = new ProviderRepository();
        var provider1 = Substitute.For<FeatureProvider>();
        provider1.Status.Returns(ProviderStatus.NotReady);

        var provider2 = Substitute.For<FeatureProvider>();
        provider2.Status.Returns(ProviderStatus.NotReady);

        var context = new EvaluationContextBuilder().Build();

        await repository.SetProviderAsync("B", provider1, context);
        await repository.SetProviderAsync("A", provider1, context);

        await repository.SetProviderAsync("A", provider2, context);
        await repository.SetProviderAsync("B", provider2, context);

        provider1.Received(1).ShutdownAsync();
    }

    [Fact]
    public async Task Can_Get_Providers_By_Name()
    {
        var repository = new ProviderRepository();
        var provider1 = Substitute.For<FeatureProvider>();
        provider1.Status.Returns(ProviderStatus.NotReady);

        var provider2 = Substitute.For<FeatureProvider>();
        provider2.Status.Returns(ProviderStatus.NotReady);

        var context = new EvaluationContextBuilder().Build();

        await repository.SetProviderAsync("A", provider1, context);
        await repository.SetProviderAsync("B", provider2, context);

        Assert.Equal(provider1, repository.GetProvider("A"));
        Assert.Equal(provider2, repository.GetProvider("B"));
    }

    [Fact]
    public async Task Replaced_Named_Provider_Gets_Latest_Set()
    {
        var repository = new ProviderRepository();
        var provider1 = Substitute.For<FeatureProvider>();
        provider1.Status.Returns(ProviderStatus.NotReady);

        var provider2 = Substitute.For<FeatureProvider>();
        provider2.Status.Returns(ProviderStatus.NotReady);

        var context = new EvaluationContextBuilder().Build();

        await repository.SetProviderAsync("A", provider1, context);
        await repository.SetProviderAsync("A", provider2, context);

        Assert.Equal(provider2, repository.GetProvider("A"));
    }

    [Fact]
    public async Task Can_Shutdown_All_Providers()
    {
        var repository = new ProviderRepository();
        var provider1 = Substitute.For<FeatureProvider>();
        provider1.Status.Returns(ProviderStatus.NotReady);

        var provider2 = Substitute.For<FeatureProvider>();
        provider2.Status.Returns(ProviderStatus.NotReady);

        var provider3 = Substitute.For<FeatureProvider>();
        provider3.Status.Returns(ProviderStatus.NotReady);

        var context = new EvaluationContextBuilder().Build();

        await repository.SetProviderAsync(provider1, context);
        await repository.SetProviderAsync("provider1", provider1, context);
        await repository.SetProviderAsync("provider2", provider2, context);
        await repository.SetProviderAsync("provider2a", provider2, context);
        await repository.SetProviderAsync("provider3", provider3, context);

        await repository.ShutdownAsync();

        provider1.Received(1).ShutdownAsync();
        provider2.Received(1).ShutdownAsync();
        provider3.Received(1).ShutdownAsync();
    }

    [Fact]
    public async Task Setting_Same_Default_Provider_Has_No_Effect()
    {
        var repository = new ProviderRepository();
        var provider = Substitute.For<FeatureProvider>();
        provider.Status.Returns(ProviderStatus.NotReady);
        var context = new EvaluationContextBuilder().Build();
        await repository.SetProviderAsync(provider, context);
        await repository.SetProviderAsync(provider, context);

        Assert.Equal(provider, repository.GetProvider());
        provider.Received(1).InitializeAsync(context);
        provider.DidNotReceive().ShutdownAsync();
    }

    [Fact]
    public async Task Setting_Null_Default_Provider_Has_No_Effect()
    {
        var repository = new ProviderRepository();
        var provider = Substitute.For<FeatureProvider>();
        provider.Status.Returns(ProviderStatus.NotReady);
        var context = new EvaluationContextBuilder().Build();
        await repository.SetProviderAsync(provider, context);
        await repository.SetProviderAsync(null, context);

        Assert.Equal(provider, repository.GetProvider());
        provider.Received(1).InitializeAsync(context);
        provider.DidNotReceive().ShutdownAsync();
    }

    [Fact]
    public async Task Setting_Null_Named_Provider_Removes_It()
    {
        var repository = new ProviderRepository();

        var namedProvider = Substitute.For<FeatureProvider>();
        namedProvider.Status.Returns(ProviderStatus.NotReady);

        var defaultProvider = Substitute.For<FeatureProvider>();
        defaultProvider.Status.Returns(ProviderStatus.NotReady);

        var context = new EvaluationContextBuilder().Build();
        await repository.SetProviderAsync(defaultProvider, context);

        await repository.SetProviderAsync("named-provider", namedProvider, context);
        await repository.SetProviderAsync("named-provider", null, context);

        Assert.Equal(defaultProvider, repository.GetProvider("named-provider"));
    }
}
