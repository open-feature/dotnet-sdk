using AutoFixture;
using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Extensions.MultiProvider;
using OpenFeature.Model;

namespace OpenFeature.Tests.Extensions.MultiProvider;

public class ComparisonStrategyTests : ClearOpenFeatureInstanceFixture
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task EvaluateAsync_Should_Return_First_Result_When_All_Agree()
    {
        // Arrange
        var strategy = new ComparisonStrategy();
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

        var expectedResult = new ResolutionDetails<bool>(flagKey, true, ErrorType.None, "STATIC", "variant");

        provider1.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);
        provider2.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task EvaluateAsync_Should_Return_First_Result_When_Values_Disagree_And_No_Fallback()
    {
        // Arrange
        var strategy = new ComparisonStrategy();
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
    public async Task EvaluateAsync_Should_Use_Fallback_Provider_When_Values_Disagree()
    {
        // Arrange
        var strategy = new ComparisonStrategy("provider2");
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
        Assert.Equal(result2, result);
    }

    [Fact]
    public async Task EvaluateAsync_Should_Call_OnMismatch_When_Values_Disagree()
    {
        // Arrange
        string? mismatchFlagKey = null;
        object? mismatchFirstValue = null;
        Dictionary<string, object?>? mismatchValues = null;

        var strategy = new ComparisonStrategy(
            fallbackProviderName: null,
            onMismatch: (flagKey, firstValue, values) =>
            {
                mismatchFlagKey = flagKey;
                mismatchFirstValue = firstValue;
                mismatchValues = values;
            });

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
        await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(flagKey, mismatchFlagKey);
        Assert.Equal(true, mismatchFirstValue);
        Assert.NotNull(mismatchValues);
        Assert.Equal(2, mismatchValues.Count);
        Assert.Equal(true, mismatchValues["provider1"]);
        Assert.Equal(false, mismatchValues["provider2"]);
    }

    [Fact]
    public async Task EvaluateAsync_Should_Return_Error_When_Provider_Has_Error()
    {
        // Arrange
        var strategy = new ComparisonStrategy();
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

        var errorResult = new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.General, "ERROR", errorMessage: "Something went wrong");
        var successResult = new ResolutionDetails<bool>(flagKey, true, ErrorType.None, "STATIC", "variant");

        provider1.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(errorResult);
        provider2.ResolveBooleanValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(successResult);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(errorResult, result);
    }

    [Fact]
    public async Task EvaluateAsync_Should_Return_FLAG_NOT_FOUND_When_No_Providers()
    {
        // Arrange
        var strategy = new ComparisonStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<bool>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var providers = new Dictionary<string, FeatureProvider>();

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(ErrorType.FlagNotFound, result.ErrorType);
        Assert.Equal("No providers available", result.ErrorMessage);
    }

    [Fact]
    public async Task EvaluateAsync_String_Should_Work_Correctly()
    {
        // Arrange
        var strategy = new ComparisonStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<string>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 }
        };

        var expectedResult = new ResolutionDetails<string>(flagKey, "test-value", ErrorType.None, "STATIC", "variant");

        provider1.ResolveStringValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);
        provider2.ResolveStringValueAsync(flagKey, defaultValue, context, cancellationToken)
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
        var strategy = new ComparisonStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<int>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 }
        };

        var expectedResult = new ResolutionDetails<int>(flagKey, 42, ErrorType.None, "STATIC", "variant");

        provider1.ResolveIntegerValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);
        provider2.ResolveIntegerValueAsync(flagKey, defaultValue, context, cancellationToken)
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
        var strategy = new ComparisonStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<double>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 }
        };

        var expectedResult = new ResolutionDetails<double>(flagKey, 3.14, ErrorType.None, "STATIC", "variant");

        provider1.ResolveDoubleValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);
        provider2.ResolveDoubleValueAsync(flagKey, defaultValue, context, cancellationToken)
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
        var strategy = new ComparisonStrategy();
        var flagKey = this._fixture.Create<string>();
        var defaultValue = this._fixture.Create<Value>();
        var context = EvaluationContext.Empty;
        var cancellationToken = CancellationToken.None;

        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        var providers = new Dictionary<string, FeatureProvider>
        {
            { "provider1", provider1 },
            { "provider2", provider2 }
        };

        var structureValue = new Value(new Structure(new Dictionary<string, Value> { { "key", new Value("value") } }));
        var expectedResult = new ResolutionDetails<Value>(flagKey, structureValue, ErrorType.None, "STATIC", "variant");

        provider1.ResolveStructureValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);
        provider2.ResolveStructureValueAsync(flagKey, defaultValue, context, cancellationToken)
            .Returns(expectedResult);

        // Act
        var result = await strategy.EvaluateAsync(providers, flagKey, defaultValue, context, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
