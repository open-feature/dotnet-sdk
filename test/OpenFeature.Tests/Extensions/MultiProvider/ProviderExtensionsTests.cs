using AutoFixture;
using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Extensions.MultiProvider;
using OpenFeature.Model;

namespace OpenFeature.Tests.Extensions.MultiProvider;

public class ProviderExtensionsTests : ClearOpenFeatureInstanceFixture
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task EvaluateAsync_Boolean_Should_Handle_Successful_Evaluation()
    {
        // Arrange
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<bool>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider = Substitute.For<FeatureProvider>();
        var expectedResult = new ResolutionDetails<bool>(flagKey, true, ErrorType.None, "STATIC", "variant");

        provider.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        // Act
        var result = await provider.EvaluateAsync(flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await provider.Received(1).ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_String_Should_Handle_Successful_Evaluation()
    {
        // Arrange
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<string>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider = Substitute.For<FeatureProvider>();
        var expectedResult = new ResolutionDetails<string>(flagKey, "success", ErrorType.None, "STATIC", "variant");

        provider.ResolveStringValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        // Act
        var result = await provider.EvaluateAsync(flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await provider.Received(1).ResolveStringValueAsync(flagKey, defaultValue, context, cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_Integer_Should_Handle_Successful_Evaluation()
    {
        // Arrange
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<int>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider = Substitute.For<FeatureProvider>();
        var expectedResult = new ResolutionDetails<int>(flagKey, 42, ErrorType.None, "STATIC", "variant");

        provider.ResolveIntegerValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        // Act
        var result = await provider.EvaluateAsync(flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await provider.Received(1).ResolveIntegerValueAsync(flagKey, defaultValue, context, cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_Double_Should_Handle_Successful_Evaluation()
    {
        // Arrange
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<double>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider = Substitute.For<FeatureProvider>();
        var expectedResult = new ResolutionDetails<double>(flagKey, 3.14, ErrorType.None, "STATIC", "variant");

        provider.ResolveDoubleValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        // Act
        var result = await provider.EvaluateAsync(flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await provider.Received(1).ResolveDoubleValueAsync(flagKey, defaultValue, context, cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_Structure_Should_Handle_Successful_Evaluation()
    {
        // Arrange
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<Value>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider = Substitute.For<FeatureProvider>();
        var successValue = new Value(Structure.Builder().Set("key", "value").Build());
        var expectedResult = new ResolutionDetails<Value>(flagKey, successValue, ErrorType.None, "STATIC", "variant");

        provider.ResolveStructureValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        // Act
        var result = await provider.EvaluateAsync(flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await provider.Received(1).ResolveStructureValueAsync(flagKey, defaultValue, context, cancellationToken);
    }
}
