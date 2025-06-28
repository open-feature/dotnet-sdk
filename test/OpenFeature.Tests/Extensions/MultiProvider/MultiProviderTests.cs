using AutoFixture;
using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;

using MultiProviderNamespace = OpenFeature.Extensions.MultiProvider;

namespace OpenFeature.Tests.Extensions.MultiProvider;

public class MultiProviderTests : ClearOpenFeatureInstanceFixture
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void MultiProvider_Should_Have_Metadata()
    {
        // Arrange
        var providers = new Dictionary<string, FeatureProvider> { { "test", new TestProvider() } };
        var strategy = new MultiProviderNamespace.FirstMatchStrategy();
        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act
        var metadata = multiProvider.GetMetadata();

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("OpenFeature MultiProvider", metadata.Name);
    }

    [Fact]
    public void Constructor_Should_Set_Properties_Correctly()
    {
        // Arrange
        var providers = new Dictionary<string, FeatureProvider> { { "test", new TestProvider() } };
        var strategy = new MultiProviderNamespace.FirstMatchStrategy();

        // Act
        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Assert
        Assert.NotNull(multiProvider);
        Assert.NotNull(multiProvider.GetMetadata());
        Assert.Equal("OpenFeature MultiProvider", multiProvider.GetMetadata().Name);
    }

    [Fact]
    public async Task ResolveBooleanValueAsync_Should_Delegate_To_Strategy()
    {
        // Arrange
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<bool>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var expectedResult = new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.None, "STATIC", "variant");

        var provider = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider> { { "test", provider } };
        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();

        strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act
        var result = await multiProvider.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await strategy.Received(1).EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);
    }

    [Fact]
    public async Task ResolveStringValueAsync_Should_Delegate_To_Strategy()
    {
        // Arrange
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<string>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var expectedResult = new ResolutionDetails<string>(flagKey, defaultValue, ErrorType.None, "STATIC", "variant");

        var provider = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider> { { "test", provider } };
        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();

        strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act
        var result = await multiProvider.ResolveStringValueAsync(flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await strategy.Received(1).EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);
    }

    [Fact]
    public async Task ResolveIntegerValueAsync_Should_Delegate_To_Strategy()
    {
        // Arrange
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<int>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var expectedResult = new ResolutionDetails<int>(flagKey, defaultValue, ErrorType.None, "STATIC", "variant");

        var provider = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider> { { "test", provider } };
        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();

        strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act
        var result = await multiProvider.ResolveIntegerValueAsync(flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await strategy.Received(1).EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);
    }

    [Fact]
    public async Task ResolveDoubleValueAsync_Should_Delegate_To_Strategy()
    {
        // Arrange
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<double>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var expectedResult = new ResolutionDetails<double>(flagKey, defaultValue, ErrorType.None, "STATIC", "variant");

        var provider = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider> { { "test", provider } };
        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();

        strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act
        var result = await multiProvider.ResolveDoubleValueAsync(flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await strategy.Received(1).EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);
    }

    [Fact]
    public async Task ResolveStructureValueAsync_Should_Delegate_To_Strategy()
    {
        // Arrange
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<Value>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var expectedResult = new ResolutionDetails<Value>(flagKey, defaultValue, ErrorType.None, "STATIC", "variant");

        var provider = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider> { { "test", provider } };
        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();

        strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act
        var result = await multiProvider.ResolveStructureValueAsync(flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await strategy.Received(1).EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);
    }

    [Fact]
    public void GetProviderHooks_Should_Return_Empty_List()
    {
        // Arrange
        var providers = new Dictionary<string, FeatureProvider> { { "test", new TestProvider() } };
        var strategy = new MultiProviderNamespace.FirstMatchStrategy();
        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act
        var hooks = multiProvider.GetProviderHooks();

        // Assert
        Assert.NotNull(hooks);
        Assert.Empty(hooks);
    }

    [Fact]
    public async Task InitializeAsync_Should_Initialize_All_Providers()
    {
        // Arrange
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var provider3 = Substitute.For<FeatureProvider>();

        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 },
            { "provider3", provider3 }
        };

        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();
        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act
        await multiProvider.InitializeAsync(context, cancellationToken);

        // Assert
        await provider1.Received(1).InitializeAsync(context, cancellationToken);
        await provider2.Received(1).InitializeAsync(context, cancellationToken);
        await provider3.Received(1).InitializeAsync(context, cancellationToken);
    }

    [Fact]
    public async Task InitializeAsync_Should_Complete_When_No_Providers()
    {
        // Arrange
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var providers = new Dictionary<string, FeatureProvider>();
        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();
        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act & Assert - Should not throw
        await multiProvider.InitializeAsync(context, cancellationToken);
    }

    [Fact]
    public async Task InitializeAsync_Should_Propagate_Exception_From_Provider()
    {
        // Arrange
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Provider initialization failed");

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();

        provider1.InitializeAsync(context, cancellationToken)
            .Returns(Task.CompletedTask);
        provider2.When(x => x.InitializeAsync(context, cancellationToken))
            .Do(x => throw expectedException);

        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 }
        };

        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();
        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => multiProvider.InitializeAsync(context, cancellationToken));

        Assert.Equal(expectedException.Message, thrownException.Message);
        await provider1.Received(1).InitializeAsync(context, cancellationToken);
        await provider2.Received(1).InitializeAsync(context, cancellationToken);
    }

    [Fact]
    public async Task ShutdownAsync_Should_Shutdown_All_Providers()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var provider3 = Substitute.For<FeatureProvider>();

        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 },
            { "provider3", provider3 }
        };

        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();
        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act
        await multiProvider.ShutdownAsync(cancellationToken);

        // Assert
        await provider1.Received(1).ShutdownAsync(cancellationToken);
        await provider2.Received(1).ShutdownAsync(cancellationToken);
        await provider3.Received(1).ShutdownAsync(cancellationToken);
    }

    [Fact]
    public async Task ShutdownAsync_Should_Complete_When_No_Providers()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var providers = new Dictionary<string, FeatureProvider>();
        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();
        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act & Assert - Should not throw
        await multiProvider.ShutdownAsync(cancellationToken);
    }

    [Fact]
    public async Task ShutdownAsync_Should_Propagate_Exception_From_Provider()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Provider shutdown failed");

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();

        provider1.ShutdownAsync(cancellationToken)
            .Returns(Task.CompletedTask);
        provider2.When(x => x.ShutdownAsync(cancellationToken))
            .Do(x => throw expectedException);

        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 }
        };

        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();
        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<AggregateException>(
            () => multiProvider.ShutdownAsync(cancellationToken));

        Assert.StartsWith("One or more providers failed to shutdown", thrownException.Message);
        Assert.Single(thrownException.InnerExceptions);
        Assert.Equal(expectedException.Message, thrownException.InnerExceptions.First().Message);
        await provider1.Received(1).ShutdownAsync(cancellationToken);
        await provider2.Received(1).ShutdownAsync(cancellationToken);
    }

    [Fact]
    public async Task ShutdownAsync_Should_Continue_All_Providers_Even_If_Some_Fail()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var provider3 = Substitute.For<FeatureProvider>();

        // Make the second provider throw an exception
        provider2.When(x => x.ShutdownAsync(cancellationToken))
            .Do(x => throw new InvalidOperationException("Provider 2 failed"));

        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 },
            { "provider3", provider3 }
        };

        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();
        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<AggregateException>(
            () => multiProvider.ShutdownAsync(cancellationToken));

        Assert.StartsWith("One or more providers failed to shutdown", thrownException.Message);
        Assert.Single(thrownException.InnerExceptions);
        Assert.Equal("Provider 2 failed", thrownException.InnerExceptions.First().Message);

        // All providers should be called even if one fails
        await provider1.Received(1).ShutdownAsync(cancellationToken);
        await provider2.Received(1).ShutdownAsync(cancellationToken);
        await provider3.Received(1).ShutdownAsync(cancellationToken);
    }

    [Fact]
    public async Task ShutdownAsync_Should_Collect_Multiple_Exceptions()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var provider3 = Substitute.For<FeatureProvider>();

        // Make multiple providers throw exceptions
        provider1.When(x => x.ShutdownAsync(cancellationToken))
            .Do(x => throw new InvalidOperationException("Provider 1 failed"));
        provider3.When(x => x.ShutdownAsync(cancellationToken))
            .Do(x => throw new ArgumentException("Provider 3 failed"));

        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 },
            { "provider3", provider3 }
        };

        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();
        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<AggregateException>(
            () => multiProvider.ShutdownAsync(cancellationToken));

        Assert.StartsWith("One or more providers failed to shutdown", thrownException.Message);
        Assert.Equal(2, thrownException.InnerExceptions.Count);
        Assert.Contains(thrownException.InnerExceptions, ex => ex.Message == "Provider 1 failed");
        Assert.Contains(thrownException.InnerExceptions, ex => ex.Message == "Provider 3 failed");

        // All providers should be called
        await provider1.Received(1).ShutdownAsync(cancellationToken);
        await provider2.Received(1).ShutdownAsync(cancellationToken);
        await provider3.Received(1).ShutdownAsync(cancellationToken);
    }

    [Fact]
    public async Task ShutdownAsync_Should_Succeed_When_All_Providers_Succeed_After_Exception_Handling_Changes()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();

        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 }
        };

        var strategy = Substitute.For<MultiProviderNamespace.BaseEvaluationStrategy>();
        var multiProvider = new MultiProviderNamespace.MultiProvider(providers, strategy);

        // Act - Should not throw
        await multiProvider.ShutdownAsync(cancellationToken);

        // Assert
        await provider1.Received(1).ShutdownAsync(cancellationToken);
        await provider2.Received(1).ShutdownAsync(cancellationToken);
    }
}
