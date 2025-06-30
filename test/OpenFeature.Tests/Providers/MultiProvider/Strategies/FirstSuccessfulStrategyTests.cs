using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Tests.Providers.MultiProvider.Strategies;

public class FirstSuccessfulStrategyTests
{
    private const string TestFlagKey = "test-flag";
    private const bool DefaultBoolValue = false;
    private const string Provider1Name = "provider1";
    private const string Provider2Name = "provider2";
    private const string Provider3Name = "provider3";
    private const string MultiProviderName = "MultiProvider";

    private readonly FirstSuccessfulStrategy _strategy = new();
    private readonly FeatureProvider _mockProvider1 = Substitute.For<FeatureProvider>();
    private readonly FeatureProvider _mockProvider2 = Substitute.For<FeatureProvider>();
    private readonly FeatureProvider _mockProvider3 = Substitute.For<FeatureProvider>();
    private readonly EvaluationContext _evaluationContext = new EvaluationContextBuilder().Build();
    private readonly StrategyEvaluationContext _strategyContext = new(TestFlagKey, typeof(bool));

    [Fact]
    public void ShouldEvaluateNextProvider_WithSuccessfulResult_ReturnsFalse()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));
        var successfulResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, "variant1"));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, successfulResult);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldEvaluateNextProvider_WithErrorResult_ReturnsTrue()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, false, ErrorType.General, Reason.Error, errorMessage: "Test error"));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, errorResult);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldEvaluateNextProvider_WithThrownException_ReturnsTrue()
    {
        // Arrange
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));
        var exceptionResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, false),
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

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, DefaultBoolValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.Equal(DefaultBoolValue, result.Details.Value);
        Assert.Equal(ErrorType.ProviderNotReady, result.Details.ErrorType);
        Assert.Equal(Reason.Error, result.Details.Reason);
        Assert.Equal("No providers available or all providers failed", result.Details.ErrorMessage);
        Assert.Equal(MultiProviderName, result.ProviderName);
        Assert.Single(result.Errors);
        Assert.Equal(MultiProviderName, result.Errors[0].ProviderName);
        Assert.IsType<InvalidOperationException>(result.Errors[0].Error);
    }

    [Fact]
    public void DetermineFinalResult_WithFirstSuccessfulResult_ReturnsFirstSuccessfulResult()
    {
        // Arrange
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, false, ErrorType.General, Reason.Error, errorMessage: "Error from provider1"));

        var successfulResult = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, "variant1"));

        var anotherSuccessfulResult = new ProviderResolutionResult<bool>(
            this._mockProvider3,
            Provider3Name,
            new ResolutionDetails<bool>(TestFlagKey, false, ErrorType.None, Reason.Static, "variant2"));

        var resolutions = new List<ProviderResolutionResult<bool>> { errorResult, successfulResult, anotherSuccessfulResult };
        const bool defaultValue = false;

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, defaultValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.True(result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
        Assert.Equal(Reason.Static, result.Details.Reason);
        Assert.Equal("variant1", result.Details.Variant);
        Assert.Equal(this._mockProvider2, result.Provider);
        Assert.Equal(Provider2Name, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DetermineFinalResult_WithAllFailedResults_ReturnsAllErrorsCollected()
    {
        // Arrange
        var errorResult1 = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, false, ErrorType.General, Reason.Error, errorMessage: "Error from provider1"));

        var errorResult2 = new ProviderResolutionResult<bool>(
            this._mockProvider2,
            Provider2Name,
            new ResolutionDetails<bool>(TestFlagKey, false, ErrorType.InvalidContext, Reason.Error, errorMessage: "Error from provider2"));

        var exceptionResult = new ProviderResolutionResult<bool>(
            this._mockProvider3,
            Provider3Name,
            new ResolutionDetails<bool>(TestFlagKey, false),
            new InvalidOperationException("Exception from provider3"));

        var resolutions = new List<ProviderResolutionResult<bool>> { errorResult1, errorResult2, exceptionResult };
        const bool defaultValue = false;

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, defaultValue, this._evaluationContext, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.Equal(defaultValue, result.Details.Value);
        Assert.Equal(ErrorType.General, result.Details.ErrorType);
        Assert.Equal(Reason.Error, result.Details.Reason);
        Assert.Equal("All providers failed", result.Details.ErrorMessage);
        Assert.Equal(MultiProviderName, result.ProviderName);
        Assert.Equal(3, result.Errors.Count);

        // Verify error from provider1
        Assert.Equal(Provider1Name, result.Errors[0].ProviderName);
        Assert.Equal("Error from provider1", result.Errors[0].Error?.Message);

        // Verify error from provider2
        Assert.Equal(Provider2Name, result.Errors[1].ProviderName);
        Assert.Equal("Error from provider2", result.Errors[1].Error?.Message);

        // Verify exception from provider3
        Assert.Equal(Provider3Name, result.Errors[2].ProviderName);
        Assert.IsType<InvalidOperationException>(result.Errors[2].Error);
        Assert.Equal("Exception from provider3", result.Errors[2].Error?.Message);
    }

    [Fact]
    public void DetermineFinalResult_WithNullEvaluationContext_HandlesGracefully()
    {
        // Arrange
        var successfulResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, "variant1"));

        var resolutions = new List<ProviderResolutionResult<bool>> { successfulResult };
        const bool defaultValue = false;

        // Act
        var result = this._strategy.DetermineFinalResult(this._strategyContext, TestFlagKey, defaultValue, null, resolutions);

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal(TestFlagKey, result.Details.FlagKey);
        Assert.True(result.Details.Value);
        Assert.Equal(ErrorType.None, result.Details.ErrorType);
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
        var strategyContext = new StrategyPerProviderContext(this._mockProvider1, Provider1Name, ProviderStatus.Ready, TestFlagKey, typeof(bool));
        var errorResult = new ProviderResolutionResult<bool>(
            this._mockProvider1,
            Provider1Name,
            new ResolutionDetails<bool>(TestFlagKey, false, errorType, Reason.Error, errorMessage: $"Error of type {errorType}"));

        // Act
        var result = this._strategy.ShouldEvaluateNextProvider(strategyContext, this._evaluationContext, errorResult);

        // Assert
        Assert.True(result);
    }
}
