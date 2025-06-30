using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Tests.Providers.MultiProvider.Strategies;

public class FirstSuccessfulStrategyTests
{
    private readonly FirstSuccessfulStrategy _strategy = new();
    private readonly FeatureProvider _mockProvider1 = Substitute.For<FeatureProvider>();
    private readonly FeatureProvider _mockProvider2 = Substitute.For<FeatureProvider>();
    private readonly FeatureProvider _mockProvider3 = Substitute.For<FeatureProvider>();
    private readonly EvaluationContext _evaluationContext = new EvaluationContextBuilder().Build();
    private readonly StrategyEvaluationContext _strategyContext = new("test-flag", typeof(bool));

    [Fact]
    public void ShouldEvaluateNextProvider_WithSuccessfulResult_ReturnsFalse()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, "provider1", ProviderStatus.Ready, "test-flag", typeof(bool));
        var successfulResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<bool>("test-flag", true, ErrorType.None, Reason.Static, "variant1"));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, successfulResult);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldEvaluateNextProvider_WithErrorResult_ReturnsTrue()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, "provider1", ProviderStatus.Ready, "test-flag", typeof(bool));
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<bool>("test-flag", false, ErrorType.General, Reason.Error, errorMessage: "Test error"));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, errorResult);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldEvaluateNextProvider_WithThrownException_ReturnsTrue()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, "provider1", ProviderStatus.Ready, "test-flag", typeof(bool));
        var exceptionResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<bool>("test-flag", false),
            new InvalidOperationException("Test exception"));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, exceptionResult);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DetermineFinalResult_WithNoProviders_ReturnsErrorResult()
    {
        // Arrange
        var resolutions = new List<ProviderResolutionResult<bool>>();
        const bool defaultValue = false;
        const string key = "test-flag";

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, key, defaultValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(key, result.Details.FlagKey);
        Assert.Equal(defaultValue, result.Details.Value);
        Assert.Equal(ErrorType.ProviderNotReady, result.Details.ErrorType);
        Assert.Equal(Reason.Error, result.Details.Reason);
        Assert.Equal("No providers available or all providers failed", result.Details.ErrorMessage);
        Assert.Equal("MultiProvider", result.ProviderName);
        Assert.Single(result.Errors);
        Assert.Equal("MultiProvider", result.Errors[0].ProviderName);
        Assert.IsType<InvalidOperationException>(result.Errors[0].Error);
    }

    [Fact]
    public void DetermineFinalResult_WithFirstSuccessfulResult_ReturnsFirstSuccessfulResult()
    {
        // Arrange
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<bool>("test-flag", false, ErrorType.General, Reason.Error, errorMessage: "Error from provider1"));

        var successfulResult = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            "provider2",
            new ResolutionDetails<bool>("test-flag", true, ErrorType.None, Reason.Static, "variant1"));

        var anotherSuccessfulResult = new ProviderResolutionResult<bool>(
            this._mockProvider3,
            "provider3",
            new ResolutionDetails<bool>("test-flag", false, ErrorType.None, Reason.Static, "variant2"));

        var resolutions = new List<ProviderResolutionResult<bool>> { errorResult, successfulResult, anotherSuccessfulResult };
        const bool defaultValue = false;
        const string key = "test-flag";

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, key, defaultValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(key, result.Details.FlagKey);
        Assert.True(result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
        Assert.Equal(Reason.Static, result.Details.Reason);
        Assert.Equal("variant1", result.Details.Variant);
        Assert.Equal(this._mockProvider2, result.Provider);
        Assert.Equal("provider2", result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DetermineFinalResult_WithAllFailedResults_ReturnsAllErrorsCollected()
    {
        // Arrange
        var errorResult1 = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<bool>("test-flag", false, ErrorType.General, Reason.Error, errorMessage: "Error from provider1"));

        var errorResult2 = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            "provider2",
            new ResolutionDetails<bool>("test-flag", false, ErrorType.InvalidContext, Reason.Error, errorMessage: "Error from provider2"));

        var exceptionResult = new ProviderResolutionResult<bool>(
            this._mockProvider3,
            "provider3",
            new ResolutionDetails<bool>("test-flag", false),
            new InvalidOperationException("Exception from provider3"));

        var resolutions = new List<ProviderResolutionResult<bool>> { errorResult1, errorResult2, exceptionResult };
        const bool defaultValue = false;
        const string key = "test-flag";

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, key, defaultValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(key, result.Details.FlagKey);
        Assert.Equal(defaultValue, result.Details.Value);
        Assert.Equal(ErrorType.General, result.Details.ErrorType);
        Assert.Equal(Reason.Error, result.Details.Reason);
        Assert.Equal("All providers failed", result.Details.ErrorMessage);
        Assert.Equal("MultiProvider", result.ProviderName);
        Assert.Equal(3, result.Errors.Count);

        // Verify error from provider1
        Assert.Equal("provider1", result.Errors[0].ProviderName);
        Assert.Equal("Error from provider1", result.Errors[0].Error?.Message);

        // Verify error from provider2
        Assert.Equal("provider2", result.Errors[1].ProviderName);
        Assert.Equal("Error from provider2", result.Errors[1].Error?.Message);

        // Verify exception from provider3
        Assert.Equal("provider3", result.Errors[2].ProviderName);
        Assert.IsType<InvalidOperationException>(result.Errors[2].Error);
        Assert.Equal("Exception from provider3", result.Errors[2].Error?.Message);
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(double))]
    public void DetermineFinalResult_WorksWithDifferentTypes<T>(Type flagType)
    {
        // Arrange
        var strategyContext = new StrategyEvaluationContext("test-flag", flagType);
        var defaultValue = default(T)!;
        T successValue = GetTestValue<T>();

        var errorResult = new ProviderResolutionResult<T>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<T>("test-flag", defaultValue, ErrorType.General, Reason.Error, errorMessage: "Error"));

        var successfulResult = new ProviderResolutionResult<T>(
            this._mockProvider2,
            "provider2",
            new ResolutionDetails<T>("test-flag", successValue, ErrorType.None, Reason.Static, "variant1"));

        var resolutions = new List<ProviderResolutionResult<T>> { errorResult, successfulResult };
        const string key = "test-flag";

        // Act
        var result = this._strategy.DetermineFinalResult(strategyContext, key, defaultValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(key, result.Details.FlagKey);
        Assert.Equal(successValue, result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
        Assert.Equal(Reason.Static, result.Details.Reason);
        Assert.Equal("variant1", result.Details.Variant);
        Assert.Equal(this._mockProvider2, result.Provider);
        Assert.Equal("provider2", result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DetermineFinalResult_WithMixedErrorTypesAndSuccess_ReturnsFirstSuccess()
    {
        // Arrange
        var flagNotFoundResult = new ProviderResolutionResult<string>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<string>("test-flag", "default", ErrorType.FlagNotFound, Reason.Error, errorMessage: "Flag not found"));

        var parseErrorResult = new ProviderResolutionResult<string>(
            this._mockProvider2,
            "provider2",
            new ResolutionDetails<string>("test-flag", "default", ErrorType.ParseError, Reason.Error, errorMessage: "Parse error"));

        var successfulResult = new ProviderResolutionResult<string>(
            this._mockProvider3,
            "provider3",
            new ResolutionDetails<string>("test-flag", "success-value", ErrorType.None, Reason.Static, "success-variant"));

        var resolutions = new List<ProviderResolutionResult<string>> { flagNotFoundResult, parseErrorResult, successfulResult };
        var strategyContext = new StrategyEvaluationContext("test-flag", typeof(string));
        const string defaultValue = "default";
        const string key = "test-flag";

        // Act
        var result = this._strategy.DetermineFinalResult(strategyContext, key, defaultValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(key, result.Details.FlagKey);
        Assert.Equal("success-value", result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
        Assert.Equal(Reason.Static, result.Details.Reason);
        Assert.Equal("success-variant", result.Details.Variant);
        Assert.Equal(this._mockProvider3, result.Provider);
        Assert.Equal("provider3", result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DetermineFinalResult_WithOnlyExceptionResults_CollectsAllExceptions()
    {
        // Arrange
        var exception1 = new ArgumentException("Argument error");
        var exception2 = new InvalidOperationException("Invalid operation");
        var exception3 = new TimeoutException("Timeout error");

        var exceptionResult1 = new ProviderResolutionResult<int>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<int>("test-flag", 0),
            exception1);

        var exceptionResult2 = new ProviderResolutionResult<int>(
            this._mockProvider2,
            "provider2",
            new ResolutionDetails<int>("test-flag", 0),
            exception2);

        var exceptionResult3 = new ProviderResolutionResult<int>(
            this._mockProvider3,
            "provider3",
            new ResolutionDetails<int>("test-flag", 0),
            exception3);

        var resolutions = new List<ProviderResolutionResult<int>> { exceptionResult1, exceptionResult2, exceptionResult3 };
        var strategyContext = new StrategyEvaluationContext("test-flag", typeof(int));
        const int defaultValue = 0;
        const string key = "test-flag";

        // Act
        var result = this._strategy.DetermineFinalResult(strategyContext, key, defaultValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(key, result.Details.FlagKey);
        Assert.Equal(defaultValue, result.Details.Value);
        Assert.Equal(ErrorType.General, result.Details.ErrorType);
        Assert.Equal(Reason.Error, result.Details.Reason);
        Assert.Equal("All providers failed", result.Details.ErrorMessage);
        Assert.Equal("MultiProvider", result.ProviderName);
        Assert.Equal(3, result.Errors.Count);

        Assert.Equal("provider1", result.Errors[0].ProviderName);
        Assert.Equal(exception1, result.Errors[0].Error);

        Assert.Equal("provider2", result.Errors[1].ProviderName);
        Assert.Equal(exception2, result.Errors[1].Error);

        Assert.Equal("provider3", result.Errors[2].ProviderName);
        Assert.Equal(exception3, result.Errors[2].Error);
    }

    [Fact]
    public void DetermineFinalResult_WithNullEvaluationContext_HandlesGracefully()
    {
        // Arrange
        var successfulResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<bool>("test-flag", true, ErrorType.None, Reason.Static, "variant1"));

        var resolutions = new List<ProviderResolutionResult<bool>> { successfulResult };
        const bool defaultValue = false;
        const string key = "test-flag";

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, key, defaultValue, null, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(key, result.Details.FlagKey);
        Assert.True(result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
    }

    [Fact]
    public void DetermineFinalResult_WithEmptyFlagKey_HandlesCorrectly()
    {
        // Arrange
        var successfulResult = new ProviderResolutionResult<string>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<string>("", "test-value", ErrorType.None, Reason.Static, "variant1"));

        var resolutions = new List<ProviderResolutionResult<string>> { successfulResult };
        var strategyContext = new StrategyEvaluationContext("", typeof(string));
        const string defaultValue = "default";
        const string key = "";

        // Act
        var result = this._strategy.DetermineFinalResult(strategyContext, key, defaultValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(key, result.Details.FlagKey);
        Assert.Equal("test-value", result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
    }

    [Fact]
    public void DetermineFinalResult_WithMultipleSuccessfulResults_ReturnsFirst()
    {
        // Arrange
        var firstSuccessResult = new ProviderResolutionResult<int>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<int>("test-flag", 100, ErrorType.None, Reason.Static, "variant1"));

        var secondSuccessResult = new ProviderResolutionResult<int>(
            this._mockProvider2,
            "provider2",
            new ResolutionDetails<int>("test-flag", 200, ErrorType.None, Reason.Static, "variant2"));

        var thirdSuccessResult = new ProviderResolutionResult<int>(
            this._mockProvider3,
            "provider3",
            new ResolutionDetails<int>("test-flag", 300, ErrorType.None, Reason.Static, "variant3"));

        var resolutions = new List<ProviderResolutionResult<int>> { firstSuccessResult, secondSuccessResult, thirdSuccessResult };
        var strategyContext = new StrategyEvaluationContext("test-flag", typeof(int));
        const int defaultValue = 0;
        const string key = "test-flag";

        // Act
        var result = this._strategy.DetermineFinalResult(strategyContext, key, defaultValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(key, result.Details.FlagKey);
        Assert.Equal(100, result.Details.Value); // Should return the first successful result
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
        Assert.Equal(Reason.Static, result.Details.Reason);
        Assert.Equal("variant1", result.Details.Variant);
        Assert.Equal(this._mockProvider1, result.Provider);
        Assert.Equal("provider1", result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(ErrorType.FlagNotFound)]
    [InlineData(ErrorType.ParseError)]
    [InlineData(ErrorType.TypeMismatch)]
    [InlineData(ErrorType.InvalidContext)]
    [InlineData(ErrorType.ProviderNotReady)]
    [InlineData(ErrorType.General)]
    public void ShouldEvaluateNextProvider_WithDifferentErrorTypes_ReturnsTrue(ErrorType errorType)
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, "provider1", ProviderStatus.Ready, "test-flag", typeof(bool));
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<bool>("test-flag", false, errorType, Reason.Error, errorMessage: $"Error of type {errorType}"));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, errorResult);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DetermineFinalResult_WithStructureType_HandlesCorrectly()
    {
        // Arrange
        var structureValue = new Value(new Structure(new Dictionary<string, Value>
        {
            ["key1"] = new Value("value1"),
            ["key2"] = new Value(42)
        }));

        var errorResult = new ProviderResolutionResult<Value>(
            this._mockProvider1,
            "provider1",
            new ResolutionDetails<Value>("test-flag", new Value(), ErrorType.General, Reason.Error, errorMessage: "Error"));

        var successfulResult = new ProviderResolutionResult<Value>(
            this._mockProvider2,
            "provider2",
            new ResolutionDetails<Value>("test-flag", structureValue, ErrorType.None, Reason.Static, "structure-variant"));

        var resolutions = new List<ProviderResolutionResult<Value>> { errorResult, successfulResult };
        var strategyContext = new StrategyEvaluationContext("test-flag", typeof(Value));
        var defaultValue = new Value();
        const string key = "test-flag";

        // Act
        var result = this._strategy.DetermineFinalResult(strategyContext, key, defaultValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(key, result.Details.FlagKey);
        Assert.Equal(structureValue, result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
        Assert.Equal(Reason.Static, result.Details.Reason);
        Assert.Equal("structure-variant", result.Details.Variant);
        Assert.Equal(this._mockProvider2, result.Provider);
        Assert.Equal("provider2", result.ProviderName);
        Assert.Empty(result.Errors);
    }

    private static T GetTestValue<T>()
    {
        return typeof(T) switch
        {
            var t when t == typeof(string) => (T)(object)"test-string",
            var t when t == typeof(int) => (T)(object)42,
            var t when t == typeof(double) => (T)(object)3.14,
            var t when t == typeof(bool) => (T)(object)true,
            _ => default!
        };
    }
}
