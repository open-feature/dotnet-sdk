using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Tests.Providers.MultiProvider.Strategies;

public class BaseEvaluationStrategyTests
{
    private const string TestFlagKey = "test-flag";
    private const bool DefaultBoolValue = false;
    private const string TestVariant = "variant1";
    private const string TestErrorMessage = "Test error message";
    private const string Provider1Name = "provider1";
    private const string Provider2Name = "provider2";

    private readonly TestableBaseEvaluationStrategy _strategy = new();
    private readonly FeatureProvider _mockProvider1 = Substitute.For<FeatureProvider>();
    private readonly FeatureProvider _mockProvider2 = Substitute.For<FeatureProvider>();
    private readonly EvaluationContext _evaluationContext = new EvaluationContextBuilder().Build();
    private readonly StrategyEvaluationContext _strategyContext = new(TestFlagKey, typeof(bool));

    [Fact]
    public void RunMode_DefaultValue_ReturnsSequential()
    {
        // Act
        var result = this._strategy.RunMode;

        // Assert
        Assert.Equal(RunMode.Sequential, result);
    }

    [Fact]
    public void ShouldEvaluateThisProvider_WithReadyProvider_ReturnsTrue()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));

        // Act
        var result = this._strategy.ShouldEvaluateThisProvider(strategyContext, this._evaluationContext);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldEvaluateThisProvider_WithNotReadyProvider_ReturnsFalse()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.NotReady, TestFlagKey, typeof(bool));

        // Act
        var result = this._strategy.ShouldEvaluateThisProvider(strategyContext, this._evaluationContext);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldEvaluateThisProvider_WithFatalProvider_ReturnsFalse()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Fatal, TestFlagKey, typeof(bool));

        // Act
        var result = this._strategy.ShouldEvaluateThisProvider(strategyContext, this._evaluationContext);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldEvaluateThisProvider_WithStaleProvider_ReturnsTrue()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Stale, TestFlagKey, typeof(bool));

        // Act
        var result = this._strategy.ShouldEvaluateThisProvider(strategyContext, this._evaluationContext);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldEvaluateThisProvider_WithNullEvaluationContext_ReturnsExpectedResult()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));

        // Act
        var result = this._strategy.ShouldEvaluateThisProvider(strategyContext, null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldEvaluateNextProvider_DefaultImplementation_ReturnsTrue()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));
        var successResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, successResult);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldEvaluateNextProvider_WithErrorResult_ReturnsTrue()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.General, Reason.Error, errorMessage: TestErrorMessage));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, errorResult);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldEvaluateNextProvider_WithNullEvaluationContext_ReturnsTrue()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));
        var successResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, null, successResult);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasError_WithThrownException_ReturnsTrue()
    {
        // Arrange
        var exception = new InvalidOperationException(TestErrorMessage);
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.None, Reason.Static),
            exception);

        // Act
        var result = TestableBaseEvaluationStrategy.TestHasError(errorResult);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasError_WithErrorType_ReturnsTrue()
    {
        // Arrange
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.General, Reason.Error, errorMessage: TestErrorMessage));

        // Act
        var result = TestableBaseEvaluationStrategy.TestHasError(errorResult);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasError_WithNoError_ReturnsFalse()
    {
        // Arrange
        var successResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        // Act
        var result = TestableBaseEvaluationStrategy.TestHasError(successResult);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CollectProviderErrors_WithThrownExceptions_ReturnsAllErrors()
    {
        // Arrange
        var exception1 = new InvalidOperationException("Error 1");
        var exception2 = new ArgumentException("Error 2");

        var resolutions = new List<ProviderResolutionResult<bool>>
        {
            new(this._mockProvider1, Provider1Name, new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.None, Reason.Static), exception1),
            new(this._mockProvider2, Provider2Name, new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.None, Reason.Static), exception2)
        };

        // Act
        var errors = TestableBaseEvaluationStrategy.TestCollectProviderErrors(resolutions);

        // Assert
        Assert.Equal(2, errors.Count);
        Assert.Equal(Provider1Name, errors[0].ProviderName);
        Assert.Equal(exception1, errors[0].Error);
        Assert.Equal(Provider2Name, errors[1].ProviderName);
        Assert.Equal(exception2, errors[1].Error);
    }

    [Fact]
    public void CollectProviderErrors_WithErrorTypes_ReturnsAllErrors()
    {
        // Arrange
        var resolutions = new List<ProviderResolutionResult<bool>>
        {
            new(this._mockProvider1, Provider1Name, new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.General, Reason.Error, errorMessage: "Error 1")),
            new(this._mockProvider2, Provider2Name, new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.FlagNotFound, Reason.Error, errorMessage: "Error 2"))
        };

        // Act
        var errors = TestableBaseEvaluationStrategy.TestCollectProviderErrors(resolutions);

        // Assert
        Assert.Equal(2, errors.Count);
        Assert.Equal(Provider1Name, errors[0].ProviderName);
        Assert.Equal("Error 1", errors[0].Error?.Message);
        Assert.Equal(Provider2Name, errors[1].ProviderName);
        Assert.Equal("Error 2", errors[1].Error?.Message);
    }

    [Fact]
    public void CollectProviderErrors_WithMixedErrors_ReturnsAllErrors()
    {
        // Arrange
        var thrownException = new InvalidOperationException("Thrown error");
        var resolutions = new List<ProviderResolutionResult<bool>>
        {
            new(this._mockProvider1, Provider1Name, new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.None, Reason.Static), thrownException),
            new(this._mockProvider2, Provider2Name, new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.General, Reason.Error, errorMessage: "Resolution error")),
            new(this._mockProvider1, "provider3", new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static))
        };

        // Act
        var errors = TestableBaseEvaluationStrategy.TestCollectProviderErrors(resolutions);

        // Assert
        Assert.Equal(2, errors.Count);
        Assert.Equal(Provider1Name, errors[0].ProviderName);
        Assert.Equal(thrownException, errors[0].Error);
        Assert.Equal(Provider2Name, errors[1].ProviderName);
        Assert.Equal("Resolution error", errors[1].Error?.Message);
    }

    [Fact]
    public void CollectProviderErrors_WithNoErrors_ReturnsEmptyList()
    {
        // Arrange
        var resolutions = new List<ProviderResolutionResult<bool>>
        {
            new(this._mockProvider1, Provider1Name, new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static)),
            new(this._mockProvider2, Provider2Name, new ResolutionDetails<bool>(TestFlagKey, false, ErrorType.None, Reason.Static))
        };

        // Act
        var errors = TestableBaseEvaluationStrategy.TestCollectProviderErrors(resolutions);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void CollectProviderErrors_WithNullErrorMessage_UsesDefaultMessage()
    {
        // Arrange
        var resolutions = new List<ProviderResolutionResult<bool>>
        {
            new(this._mockProvider1, Provider1Name, new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.General, Reason.Error, errorMessage: null))
        };

        // Act
        var errors = TestableBaseEvaluationStrategy.TestCollectProviderErrors(resolutions);

        // Assert
        Assert.Single(errors);
        Assert.Equal("unknown error", errors[0].Error?.Message);
    }

    [Fact]
    public void HasErrorWithCode_WithMatchingErrorType_ReturnsTrue()
    {
        // Arrange
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.FlagNotFound, Reason.Error, errorMessage: TestErrorMessage));

        // Act
        var result = TestableBaseEvaluationStrategy.TestHasErrorWithCode(errorResult, ErrorType.FlagNotFound);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasErrorWithCode_WithDifferentErrorType_ReturnsFalse()
    {
        // Arrange
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.General, Reason.Error, errorMessage: TestErrorMessage));

        // Act
        var result = TestableBaseEvaluationStrategy.TestHasErrorWithCode(errorResult, ErrorType.FlagNotFound);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasErrorWithCode_WithNoError_ReturnsFalse()
    {
        // Arrange
        var successResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        // Act
        var result = TestableBaseEvaluationStrategy.TestHasErrorWithCode(successResult, ErrorType.FlagNotFound);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasErrorWithCode_WithThrownException_ReturnsFalse()
    {
        // Arrange
        var exception = new InvalidOperationException(TestErrorMessage);
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.None, Reason.Static),
            exception);

        // Act
        var result = TestableBaseEvaluationStrategy.TestHasErrorWithCode(errorResult, ErrorType.General);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToFinalResult_WithSuccessResult_ReturnsCorrectFinalResult()
    {
        // Arrange
        var resolutionDetails = new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant);
        var providerResult = new ProviderResolutionResult<bool>(this._mockProvider1, Provider1Name, resolutionDetails);

        // Act
        var finalResult = TestableBaseEvaluationStrategy.TestToFinalResult(providerResult);

        // Assert
        Assert.Equal(resolutionDetails, finalResult.Details);
        Assert.Equal(this._mockProvider1, finalResult.Provider);
        Assert.Equal(Provider1Name, finalResult.ProviderName);
        Assert.Empty(finalResult.Errors);
    }

    [Fact]
    public void ToFinalResult_WithErrorResult_ReturnsCorrectFinalResult()
    {
        // Arrange
        var resolutionDetails = new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.General, Reason.Error, errorMessage: TestErrorMessage);
        var providerResult = new ProviderResolutionResult<bool>(this._mockProvider1, Provider1Name, resolutionDetails);

        // Act
        var finalResult = TestableBaseEvaluationStrategy.TestToFinalResult(providerResult);

        // Assert
        Assert.Equal(resolutionDetails, finalResult.Details);
        Assert.Equal(this._mockProvider1, finalResult.Provider);
        Assert.Equal(Provider1Name, finalResult.ProviderName);
        Assert.Empty(finalResult.Errors);
    }

    [Theory]
    [InlineData(ProviderStatus.Ready)]
    [InlineData(ProviderStatus.Stale)]
    [InlineData(ProviderStatus.Error)]
    public void ShouldEvaluateThisProvider_WithAllowedStatuses_ReturnsTrue(ProviderStatus status)
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, status, TestFlagKey, typeof(bool));

        // Act
        var result = this._strategy.ShouldEvaluateThisProvider(strategyContext, this._evaluationContext);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(ProviderStatus.NotReady)]
    [InlineData(ProviderStatus.Fatal)]
    public void ShouldEvaluateThisProvider_WithDisallowedStatuses_ReturnsFalse(ProviderStatus status)
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, status, TestFlagKey, typeof(bool));

        // Act
        var result = this._strategy.ShouldEvaluateThisProvider(strategyContext, this._evaluationContext);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(ErrorType.None)]
    [InlineData(ErrorType.FlagNotFound)]
    [InlineData(ErrorType.General)]
    [InlineData(ErrorType.ParseError)]
    [InlineData(ErrorType.TypeMismatch)]
    [InlineData(ErrorType.TargetingKeyMissing)]
    [InlineData(ErrorType.InvalidContext)]
    [InlineData(ErrorType.ProviderNotReady)]
    public void HasErrorWithCode_WithAllErrorTypes_ReturnsCorrectResult(ErrorType errorType)
    {
        // Arrange
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, errorType, Reason.Error, errorMessage: TestErrorMessage));

        // Act
        var result = TestableBaseEvaluationStrategy.TestHasErrorWithCode(errorResult, errorType);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DetermineFinalResult_IsAbstractMethod_RequiresImplementation()
    {
        // This test verifies that DetermineFinalResult is abstract and must be implemented
        // by testing our concrete implementation

        // Arrange
        var resolutions = new List<ProviderResolutionResult<bool>>
        {
            new(this._mockProvider1, Provider1Name, new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant))
        };

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestImplementation", result.ProviderName); // From our test implementation
    }

    /// <summary>
    /// Concrete implementation of BaseEvaluationStrategy for testing purposes.
    /// </summary>
    private class TestableBaseEvaluationStrategy : BaseEvaluationStrategy
    {
        public override FinalResult<T> DetermineFinalResult<T>(StrategyEvaluationContext strategyContext, string key, T defaultValue, EvaluationContext? evaluationContext, List<ProviderResolutionResult<T>> resolutions)
        {
            // Simple test implementation that returns the first result or a default
            if (resolutions.Count > 0)
            {
                return new FinalResult<T>(resolutions[0].ResolutionDetails, resolutions[0].Provider, "TestImplementation", null);
            }

            var defaultDetails = new ResolutionDetails<T>(key, defaultValue, ErrorType.None, Reason.Default);
            return new FinalResult<T>(defaultDetails, null!, "TestImplementation", null);
        }

        // Expose protected methods for testing
        public static bool TestHasError<T>(ProviderResolutionResult<T> resolution) => HasError(resolution);
        public static List<ProviderError> TestCollectProviderErrors<T>(List<ProviderResolutionResult<T>> resolutions) => CollectProviderErrors(resolutions);
        public static bool TestHasErrorWithCode<T>(ProviderResolutionResult<T> resolution, ErrorType errorType) => HasErrorWithCode(resolution, errorType);
        public static FinalResult<T> TestToFinalResult<T>(ProviderResolutionResult<T> resolution) => ToFinalResult(resolution);
    }
}
