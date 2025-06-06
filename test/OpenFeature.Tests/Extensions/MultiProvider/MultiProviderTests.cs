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
}
