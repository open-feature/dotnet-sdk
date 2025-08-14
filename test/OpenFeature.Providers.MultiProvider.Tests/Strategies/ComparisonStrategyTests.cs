using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Providers.MultiProvider.Tests.Strategies;

public class ComparisonStrategyTests
{
    private const string TestFlagKey = "test-flag";
    private const bool DefaultBoolValue = false;
    private const string TestVariant = "variant1";
    private const string TestErrorMessage = "Test error message";
    private const string Provider1Name = "provider1";
    private const string Provider2Name = "provider2";
    private const string Provider3Name = "provider3";
    private const string MultiProviderName = "MultiProvider";

    private readonly FeatureProvider _mockProvider1 = Substitute.For<FeatureProvider>();
    private readonly FeatureProvider _mockProvider2 = Substitute.For<FeatureProvider>();
    private readonly FeatureProvider _mockProvider3 = Substitute.For<FeatureProvider>();
    private readonly EvaluationContext _evaluationContext = new EvaluationContextBuilder().Build();
    private readonly StrategyEvaluationContext<bool> _strategyContext = new(TestFlagKey);

    [Fact]
    public void RunMode_ReturnsParallel()
    {
        // Arrange
        var strategy = new ComparisonStrategy();

        // Act
        var result = strategy.RunMode;

        // Assert
        Assert.Equal(RunMode.Parallel, result);
    }

    [Fact]
    public void Constructor_WithNoParameters_InitializesSuccessfully()
    {
        // Act
        var strategy = new ComparisonStrategy();

        // Assert
        Assert.NotNull(strategy);
        Assert.Equal(RunMode.Parallel, strategy.RunMode);
    }

    [Fact]
    public void Constructor_WithFallbackProvider_InitializesSuccessfully()
    {
        // Act
        var strategy = new ComparisonStrategy(this._mockProvider1);

        // Assert
        Assert.NotNull(strategy);
        Assert.Equal(RunMode.Parallel, strategy.RunMode);
    }

    [Fact]
    public void Constructor_WithOnMismatchCallback_InitializesSuccessfully()
    {
        // Arrange
        var onMismatch = Substitute.For<Action<IDictionary<string, object>>>();

        // Act
        var strategy = new ComparisonStrategy(onMismatch: onMismatch);

        // Assert
        Assert.NotNull(strategy);
        Assert.Equal(RunMode.Parallel, strategy.RunMode);
    }

    [Fact]
    public void Constructor_WithBothParameters_InitializesSuccessfully()
    {
        // Arrange
        var onMismatch = Substitute.For<Action<IDictionary<string, object>>>();

        // Act
        var strategy = new ComparisonStrategy(this._mockProvider1, onMismatch);

        // Assert
        Assert.NotNull(strategy);
        Assert.Equal(RunMode.Parallel, strategy.RunMode);
    }

    [Fact]
    public void DetermineFinalResult_WithNoProviders_ReturnsErrorResult()
    {
        // Arrange
        var strategy = new ComparisonStrategy();
        var resolutions = new List<ProviderResolutionResult<bool>>();

        // Act
        var result = strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.Equal(DefaultBoolValue, result.Details.Value);
        Assert.Equal(ErrorType.ProviderNotReady, result.Details.ErrorType);
        Assert.Equal(Reason.Error, result.Details.Reason);
        Assert.Equal("No providers available or all providers failed", result.Details.ErrorMessage);
        Assert.Equal(MultiProviderName, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DetermineFinalResult_WithAllFailedProviders_ReturnsErrorResult()
    {
        // Arrange
        var strategy = new ComparisonStrategy();
        var errorResult1 = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.General, Reason.Error, errorMessage: "Error from provider1"));

        var errorResult2 = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.InvalidContext, Reason.Error, errorMessage: "Error from provider2"));

        var resolutions = new List<ProviderResolutionResult<bool>> { errorResult1, errorResult2 };

        // Act
        var result = strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.Equal(DefaultBoolValue, result.Details.Value);
        Assert.Equal(ErrorType.ProviderNotReady, result.Details.ErrorType);
        Assert.Equal(Reason.Error, result.Details.Reason);
        Assert.Equal("No providers available or all providers failed", result.Details.ErrorMessage);
        Assert.Equal(MultiProviderName, result.ProviderName);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void DetermineFinalResult_WithSingleSuccessfulProvider_ReturnsResult()
    {
        // Arrange
        var strategy = new ComparisonStrategy();
        var successfulResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        var resolutions = new List<ProviderResolutionResult<bool>> { successfulResult };

        // Act
        var result = strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

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
    public void DetermineFinalResult_WithAgreingProviders_ReturnsFirstResult()
    {
        // Arrange
        var strategy = new ComparisonStrategy();
        var result1 = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        var result2 = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, "variant2"));

        var result3 = new ProviderResolutionResult<bool>(
            this._mockProvider3,
            Provider3Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, "variant3"));

        var resolutions = new List<ProviderResolutionResult<bool>> { result1, result2, result3 };

        // Act
        var result = strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

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
    public void DetermineFinalResult_WithDisagreeingProviders_ReturnsFirstResult()
    {
        // Arrange
        var strategy = new ComparisonStrategy();
        var result1 = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        var result2 = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, false, ErrorType.None, Reason.Static, "variant2"));

        var resolutions = new List<ProviderResolutionResult<bool>> { result1, result2 };

        // Act
        var result = strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

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
    public void DetermineFinalResult_WithDisagreeingProvidersAndFallback_ReturnsFallbackResult()
    {
        // Arrange
        var strategy = new ComparisonStrategy(this._mockProvider2);
        var result1 = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        var result2 = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, false, ErrorType.None, Reason.Static, "variant2"));

        var resolutions = new List<ProviderResolutionResult<bool>> { result1, result2 };

        // Act
        var result = strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.False(result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
        Assert.Equal(Reason.Static, result.Details.Reason);
        Assert.Equal("variant2", result.Details.Variant);
        Assert.Equal(this._mockProvider2, result.Provider);
        Assert.Equal(Provider2Name, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DetermineFinalResult_WithDisagreeingProvidersAndNonExistentFallback_ReturnsFirstResult()
    {
        // Arrange
        var nonExistentProvider = Substitute.For<FeatureProvider>();
        var strategy = new ComparisonStrategy(nonExistentProvider);
        var result1 = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        var result2 = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, false, ErrorType.None, Reason.Static, "variant2"));

        var resolutions = new List<ProviderResolutionResult<bool>> { result1, result2 };

        // Act
        var result = strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

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
    public void DetermineFinalResult_WithDisagreeingProvidersAndOnMismatchCallback_CallsCallback()
    {
        // Arrange
        var onMismatchCalled = false;
        IDictionary<string, object>? capturedMismatchDetails = null;

        var onMismatch = new Action<IDictionary<string, object>>(details =>
        {
            onMismatchCalled = true;
            capturedMismatchDetails = details;
        });

        var strategy = new ComparisonStrategy(onMismatch: onMismatch);
        var result1 = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        var result2 = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, false, ErrorType.None, Reason.Static, "variant2"));

        var resolutions = new List<ProviderResolutionResult<bool>> { result1, result2 };

        // Act
        strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.True(onMismatchCalled);
        Assert.NotNull(capturedMismatchDetails);
        Assert.Equal(2, capturedMismatchDetails.Count);
        Assert.True((bool)capturedMismatchDetails[Provider1Name]);
        Assert.False((bool)capturedMismatchDetails[Provider2Name]);
    }

    [Fact]
    public void DetermineFinalResult_WithAgreingProvidersAndOnMismatchCallback_DoesNotCallCallback()
    {
        // Arrange
        var onMismatchCalled = false;

        var onMismatch = new Action<IDictionary<string, object>>(_ =>
        {
            onMismatchCalled = true;
        });

        var strategy = new ComparisonStrategy(onMismatch: onMismatch);
        var result1 = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        var result2 = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, "variant2"));

        var resolutions = new List<ProviderResolutionResult<bool>> { result1, result2 };

        // Act
        strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.False(onMismatchCalled);
    }

    [Fact]
    public void DetermineFinalResult_WithMixedSuccessAndErrorResults_IgnoresErrors()
    {
        // Arrange
        var strategy = new ComparisonStrategy();
        var successfulResult1 = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.General, Reason.Error, errorMessage: TestErrorMessage));

        var successfulResult2 = new ProviderResolutionResult<bool>(
            this._mockProvider3,
            Provider3Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, "variant3"));

        var resolutions = new List<ProviderResolutionResult<bool>> { successfulResult1, errorResult, successfulResult2 };

        // Act
        var result = strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

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
    public void DetermineFinalResult_WithFallbackProviderAndBothSuccessfulAndFallbackAgree_ReturnsFallbackResult()
    {
        // Arrange
        var strategy = new ComparisonStrategy(this._mockProvider2);
        var result1 = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        var fallbackResult = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, "variant2"));

        var resolutions = new List<ProviderResolutionResult<bool>> { result1, fallbackResult };

        // Act
        var result = strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.True(result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
        Assert.Equal(Reason.Static, result.Details.Reason);
        Assert.Equal(TestVariant, result.Details.Variant); // Returns first result when all agree
        Assert.Equal(this._mockProvider1, result.Provider);
        Assert.Equal(Provider1Name, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DetermineFinalResult_WithFallbackProviderHavingError_UsesFallbackWhenAvailable()
    {
        // Arrange
        var strategy = new ComparisonStrategy(this._mockProvider1);
        var result1 = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant));

        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, DefaultBoolValue, ErrorType.General, Reason.Error, errorMessage: TestErrorMessage));

        var result3 = new ProviderResolutionResult<bool>(
            this._mockProvider3,
            Provider3Name,
            new ResolutionDetails<bool>(TestFlagKey, false, ErrorType.None, Reason.Static, "variant3"));

        var resolutions = new List<ProviderResolutionResult<bool>> { result1, errorResult, result3 };

        // Act
        var result = strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

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
}
