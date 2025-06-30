using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Tests.Providers.MultiProvider.Strategies;

public class FirstMatchStrategyTests
{
    private const string TestFlagKey = "test-flag";
    private const bool DefaultBoolValue = false;
    private const string TestVariant = "variant1";
    private const string TestErrorMessage = "Test error message";
    private const string Provider1Name = "provider1";
    private const string Provider2Name = "provider2";
    private const string MultiProviderName = "MultiProvider";
    private const string NoProvidersErrorMessage = "No providers available or all providers failed";

    private readonly FirstMatchStrategy _strategy = new();
    private readonly FeatureProvider _mockProvider1 = Substitute.For<FeatureProvider>();
    private readonly FeatureProvider _mockProvider2 = Substitute.For<FeatureProvider>();
    private readonly EvaluationContext _evaluationContext = new EvaluationContextBuilder().Build();
    private readonly StrategyEvaluationContext _strategyContext = new(TestFlagKey, typeof(bool));

    [Fact]
    public void ShouldEvaluateNextProvider_WithFlagNotFoundError_ReturnsTrue()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));
        var flagNotFoundResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.FlagNotFound, Reason.Error, errorMessage: "Flag not found"));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, flagNotFoundResult);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldEvaluateNextProvider_WithSuccessfulResult_ReturnsFalse()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));
        var successfulResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, successfulResult);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldEvaluateNextProvider_WithGeneralError_ReturnsFalse()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));
        var generalErrorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.General, Reason.Error, errorMessage: TestErrorMessage));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, generalErrorResult);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldEvaluateNextProvider_WithInvalidContextError_ReturnsFalse()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));
        var invalidContextResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.InvalidContext, Reason.Error, errorMessage: "Invalid context"));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, invalidContextResult);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldEvaluateNextProvider_WithThrownException_ReturnsFalse()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));
        var exceptionResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue),
            new InvalidOperationException("Test exception"));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, exceptionResult);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DetermineFinalResult_WithNoProviders_ReturnsErrorResult()
    {
        // Arrange
        var resolutions = new List<ProviderResolutionResult<bool>>();

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.Equal(DefaultBoolValue, result.Details.Value);
        Assert.Equal(ErrorType.ProviderNotReady, result.Details.ErrorType);
        Assert.Equal(Reason.Error, result.Details.Reason);
        Assert.Equal(NoProvidersErrorMessage, result.Details.ErrorMessage);
        Assert.Equal(MultiProviderName, result.ProviderName);
        Assert.Single(result.Errors);
        Assert.Equal(MultiProviderName, result.Errors[0].ProviderName);
        Assert.IsType<InvalidOperationException>(result.Errors[0].Error);
        Assert.Equal(NoProvidersErrorMessage, result.Errors[0].Error?.Message);
    }

    [Fact]
    public void DetermineFinalResult_WithSingleSuccessfulResult_ReturnsLastResult()
    {
        // Arrange
        var successfulResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        var resolutions = new List<ProviderResolutionResult<bool>> { successfulResult };

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.True(result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
        Assert.Equal(Reason.Static, result.Details.Reason);
        Assert.Equal(TestVariant, result.Details.Variant);
        Assert.Equal(this._mockProvider1, result.Provider);
        Assert.Equal(Provider1Name, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DetermineFinalResult_WithMultipleResults_ReturnsLastResult()
    {
        // Arrange
        var flagNotFoundResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.FlagNotFound, Reason.Error, errorMessage: "Flag not found"));

        var successfulResult = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        var resolutions = new List<ProviderResolutionResult<bool>> { flagNotFoundResult, successfulResult };

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.True(result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
        Assert.Equal(Reason.Static, result.Details.Reason);
        Assert.Equal(TestVariant, result.Details.Variant);
        Assert.Equal(this._mockProvider2, result.Provider);
        Assert.Equal(Provider2Name, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DetermineFinalResult_WithLastResultHavingError_ReturnsLastResultWithError()
    {
        // Arrange
        var flagNotFoundResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.FlagNotFound, Reason.Error, errorMessage: "Flag not found"));

        var generalErrorResult = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.General, Reason.Error, errorMessage: TestErrorMessage));

        var resolutions = new List<ProviderResolutionResult<bool>> { flagNotFoundResult, generalErrorResult };

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.Equal(DefaultBoolValue, result.Details.Value);
        Assert.Equal(ErrorType.General, result.Details.ErrorType);
        Assert.Equal(Reason.Error, result.Details.Reason);
        Assert.Equal(TestErrorMessage, result.Details.ErrorMessage);
        Assert.Equal(this._mockProvider2, result.Provider);
        Assert.Equal(Provider2Name, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DetermineFinalResult_WithLastResultHavingException_ReturnsLastResultWithException()
    {
        // Arrange
        var flagNotFoundResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.FlagNotFound, Reason.Error, errorMessage: "Flag not found"));

        var exceptionResult = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue),
            new ArgumentException("Test argument exception"));

        var resolutions = new List<ProviderResolutionResult<bool>> { flagNotFoundResult, exceptionResult };

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.Equal(DefaultBoolValue, result.Details.Value);
        Assert.Equal(this._mockProvider2, result.Provider);
        Assert.Equal(Provider2Name, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DetermineFinalResult_WithStringType_ReturnsCorrectType()
    {
        // Arrange
        const string defaultStringValue = "default";
        const string testStringValue = "test-value";
        const string stringVariant = "string-variant";

        var successfulResult = new ProviderResolutionResult<string>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<string>(TestFlagKey, testStringValue, ErrorType.None, Reason.Static, stringVariant));

        var resolutions = new List<ProviderResolutionResult<string>> { successfulResult };
        var stringStrategyContext = new StrategyEvaluationContext(TestFlagKey, typeof(string));

        // Act
        var result = this._strategy.DetermineFinalResult(stringStrategyContext, TestFlagKey, defaultStringValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.Equal(testStringValue, result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
        Assert.Equal(Reason.Static, result.Details.Reason);
        Assert.Equal(stringVariant, result.Details.Variant);
        Assert.Equal(this._mockProvider1, result.Provider);
        Assert.Equal(Provider1Name, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DetermineFinalResult_WithIntType_ReturnsCorrectType()
    {
        // Arrange
        const int defaultIntValue = 0;
        const int testIntValue = 42;
        const string intVariant = "int-variant";

        var successfulResult = new ProviderResolutionResult<int>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<int>(TestFlagKey, testIntValue, ErrorType.None, Reason.Static, intVariant));

        var resolutions = new List<ProviderResolutionResult<int>> { successfulResult };
        var intStrategyContext = new StrategyEvaluationContext(TestFlagKey, typeof(int));

        // Act
        var result = this._strategy.DetermineFinalResult(intStrategyContext, TestFlagKey, defaultIntValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.Equal(testIntValue, result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
        Assert.Equal(Reason.Static, result.Details.Reason);
        Assert.Equal(intVariant, result.Details.Variant);
        Assert.Equal(this._mockProvider1, result.Provider);
        Assert.Equal(Provider1Name, result.ProviderName);
        Assert.Empty(result.Errors);
    }
}
