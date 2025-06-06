using AutoFixture;
using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Extensions.MultiProvider;
using OpenFeature.Model;

namespace OpenFeature.Tests.Extensions.MultiProvider;

public class FirstSuccessfulStrategyTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task EvaluateAsync_Should_Return_First_Non_FLAG_NOT_FOUND_Result()
    {
        // Arrange
        var strategy = new FirstSuccessfulStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<bool>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 }
        };

        var flagNotFoundResult = new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.FlagNotFound, "FLAG_NOT_FOUND");
        var successResult = new ResolutionDetails<bool>(flagKey, true, ErrorType.None, "STATIC", "variant");

        provider1.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(flagNotFoundResult);
        provider2.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(successResult);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(successResult, result);
    }

    [Fact]
    public async Task EvaluateAsync_Should_Return_FLAG_NOT_FOUND_When_All_Providers_Return_FLAG_NOT_FOUND()
    {
        // Arrange
        var strategy = new FirstSuccessfulStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<bool>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 }
        };

        var flagNotFoundResult = new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.FlagNotFound, "FLAG_NOT_FOUND");

        provider1.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(flagNotFoundResult);
        provider2.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(flagNotFoundResult);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(ErrorType.FlagNotFound, result.ErrorType);
    }

    [Fact]
    public async Task EvaluateAsync_Should_Return_First_Result_When_No_FLAG_NOT_FOUND()
    {
        // Arrange
        var strategy = new FirstSuccessfulStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<bool>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 }
        };

        var result1 = new ResolutionDetails<bool>(flagKey, true, ErrorType.None, "STATIC", "variant1");
        var result2 = new ResolutionDetails<bool>(flagKey, false, ErrorType.None, "STATIC", "variant2");

        provider1.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(result1);
        provider2.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(result2);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(result1, result);
    }

    [Fact]
    public async Task EvaluateAsync_String_Should_Work_Correctly()
    {
        // Arrange
        var strategy = new FirstSuccessfulStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<string>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider> { { "provider", provider } };

        var expectedResult = new ResolutionDetails<string>(flagKey, "test-value", ErrorType.None, "STATIC", "variant");

        provider.ResolveStringValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task EvaluateAsync_Integer_Should_Work_Correctly()
    {
        // Arrange
        var strategy = new FirstSuccessfulStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<int>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider> { { "provider", provider } };

        var expectedResult = new ResolutionDetails<int>(flagKey, 42, ErrorType.None, "STATIC", "variant");

        provider.ResolveIntegerValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task EvaluateAsync_Double_Should_Work_Correctly()
    {
        // Arrange
        var strategy = new FirstSuccessfulStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<double>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider> { { "provider", provider } };

        var expectedResult = new ResolutionDetails<double>(flagKey, 3.14, ErrorType.None, "STATIC", "variant");

        provider.ResolveDoubleValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task EvaluateAsync_Structure_Should_Work_Correctly()
    {
        // Arrange
        var strategy = new FirstSuccessfulStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<Value>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider> { { "provider", provider } };

        var structureValue = new Value(new Structure(new Dictionary<string, Value> { { "key", new Value("value") } }));
        var expectedResult = new ResolutionDetails<Value>(flagKey, structureValue, ErrorType.None, "STATIC", "variant");

        provider.ResolveStructureValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task EvaluateAsync_Should_Continue_FLAG_NOT_FOUND_Error()
    {
        // Arrange
        var strategy = new FirstSuccessfulStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<bool>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider> { { "provider", provider } };

        var flagNotFoundResult = new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.FlagNotFound, "FLAG_NOT_FOUND");

        provider.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(flagNotFoundResult);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(ErrorType.FlagNotFound, result.ErrorType);
        Assert.Equal(Reason.Error, result.Reason);
        Assert.Equal("Flag not found in any provider", result.ErrorMessage);
    }

    [Fact]
    public async Task EvaluateAsync_Should_Continue_On_FLAG_NOT_FOUND_And_Continue_Other_Errors()
    {
        // Arrange
        var strategy = new FirstSuccessfulStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<bool>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 }
        };

        var flagNotFoundResult = new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.FlagNotFound, "FLAG_NOT_FOUND");
        var errorResult = new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.TypeMismatch, "TYPE_MISMATCH");

        provider1.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(flagNotFoundResult);
        provider2.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(errorResult);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(ErrorType.FlagNotFound, result.ErrorType);
        Assert.Equal(Reason.Error, result.Reason);
        Assert.Equal("Flag not found in any provider", result.ErrorMessage);
    }

    [Fact]
    public async Task EvaluateAsync_Should_Respect_Provider_Order()
    {
        // Arrange
        var strategy = new FirstSuccessfulStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<bool>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 }
        };

        var result1 = new ResolutionDetails<bool>(flagKey, true, ErrorType.None, "STATIC", "variant1");
        var result2 = new ResolutionDetails<bool>(flagKey, false, ErrorType.None, "STATIC", "variant2");

        provider1.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(result1);
        provider2.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(result2);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(result1, result);
        await provider1.Received(1).ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken);
        await provider2.DidNotReceive().ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken);
    }
}
