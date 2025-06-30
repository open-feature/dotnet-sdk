using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Tests.Providers.MultiProvider;

public class ProviderExtensionsTests
{
    private const string TestFlagKey = "test-flag";
    private const string TestProviderName = "test-provider";
    private const string TestVariant = "test-variant";

    private readonly FeatureProvider _mockProvider = Substitute.For<FeatureProvider>();
    private readonly EvaluationContext _evaluationContext = new EvaluationContextBuilder().Build();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    [Fact]
    public async Task EvaluateAsync_WithBooleanType_CallsResolveBooleanValueAsync()
    {
        // Arrange
        const bool defaultValue = false;
        const bool resolvedValue = true;
        var expectedDetails = new ResolutionDetails<bool>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey, typeof(bool));

        this._mockProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Equal(expectedDetails, result.ResolutionDetails);
        Assert.Null(result.ThrownError);
        await this._mockProvider.Received(1).ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_WithStringType_CallsResolveStringValueAsync()
    {
        // Arrange
        const string defaultValue = "default";
        const string resolvedValue = "resolved";
        var expectedDetails = new ResolutionDetails<string>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey, typeof(string));

        this._mockProvider.ResolveStringValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Equal(expectedDetails, result.ResolutionDetails);
        Assert.Null(result.ThrownError);
        await this._mockProvider.Received(1).ResolveStringValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_WithIntegerType_CallsResolveIntegerValueAsync()
    {
        // Arrange
        const int defaultValue = 0;
        const int resolvedValue = 42;
        var expectedDetails = new ResolutionDetails<int>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey, typeof(int));

        this._mockProvider.ResolveIntegerValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Equal(expectedDetails, result.ResolutionDetails);
        Assert.Null(result.ThrownError);
        await this._mockProvider.Received(1).ResolveIntegerValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_WithDoubleType_CallsResolveDoubleValueAsync()
    {
        // Arrange
        const double defaultValue = 0.0;
        const double resolvedValue = 3.14;
        var expectedDetails = new ResolutionDetails<double>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey, typeof(double));

        this._mockProvider.ResolveDoubleValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Equal(expectedDetails, result.ResolutionDetails);
        Assert.Null(result.ThrownError);
        await this._mockProvider.Received(1).ResolveDoubleValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_WithValueType_CallsResolveStructureValueAsync()
    {
        // Arrange
        var defaultValue = new Value();
        var resolvedValue = new Value("resolved");
        var expectedDetails = new ResolutionDetails<Value>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey, typeof(Value));

        this._mockProvider.ResolveStructureValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Equal(expectedDetails, result.ResolutionDetails);
        Assert.Null(result.ThrownError);
        await this._mockProvider.Received(1).ResolveStructureValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_WithUnsupportedType_ThrowsArgumentException()
    {
        // Arrange
        var defaultValue = new DateTime(2023, 1, 1);
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey, typeof(DateTime));

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Equal(TestFlagKey, result.ResolutionDetails.FlagKey);
        Assert.Equal(defaultValue, result.ResolutionDetails.Value);
        Assert.Equal(ErrorType.General, result.ResolutionDetails.ErrorType);
        Assert.Equal(Reason.Error, result.ResolutionDetails.Reason);
        Assert.Contains("Unsupported flag type", result.ResolutionDetails.ErrorMessage);
        Assert.NotNull(result.ThrownError);
        Assert.IsType<ArgumentException>(result.ThrownError);
    }

    [Fact]
    public async Task EvaluateAsync_WhenProviderThrowsException_ReturnsErrorResult()
    {
        // Arrange
        const bool defaultValue = false;
        var expectedException = new InvalidOperationException("Provider error");
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey, typeof(bool));

        this._mockProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .ThrowsAsync(expectedException);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Equal(TestFlagKey, result.ResolutionDetails.FlagKey);
        Assert.Equal(defaultValue, result.ResolutionDetails.Value);
        Assert.Equal(ErrorType.General, result.ResolutionDetails.ErrorType);
        Assert.Equal(Reason.Error, result.ResolutionDetails.Reason);
        Assert.Equal("Provider error", result.ResolutionDetails.ErrorMessage);
        Assert.Equal(expectedException, result.ThrownError);
    }

    [Fact]
    public async Task EvaluateAsync_WithNullEvaluationContext_CallsProviderWithNullContext()
    {
        // Arrange
        const bool defaultValue = false;
        const bool resolvedValue = true;
        var expectedDetails = new ResolutionDetails<bool>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey, typeof(bool));

        this._mockProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, null, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, null, defaultValue, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Equal(expectedDetails, result.ResolutionDetails);
        Assert.Null(result.ThrownError);
        await this._mockProvider.Received(1).ResolveBooleanValueAsync(TestFlagKey, defaultValue, null, this._cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_WithCancellationToken_PassesToProvider()
    {
        // Arrange
        const string defaultValue = "default";
        const string resolvedValue = "resolved";
        var expectedDetails = new ResolutionDetails<string>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey, typeof(string));
        var customCancellationToken = new CancellationTokenSource().Token;

        this._mockProvider.ResolveStringValueAsync(TestFlagKey, defaultValue, this._evaluationContext, customCancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, customCancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDetails, result.ResolutionDetails);
        await this._mockProvider.Received(1).ResolveStringValueAsync(TestFlagKey, defaultValue, this._evaluationContext, customCancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_WithNullDefaultValue_PassesNullToProvider()
    {
        // Arrange
        string? defaultValue = null;
        const string resolvedValue = "resolved";
        var expectedDetails = new ResolutionDetails<string>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey, typeof(string));

        this._mockProvider.ResolveStringValueAsync(TestFlagKey, defaultValue!, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue!, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDetails, result.ResolutionDetails);
        await this._mockProvider.Received(1).ResolveStringValueAsync(TestFlagKey, defaultValue!, this._evaluationContext, this._cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_WithDifferentFlagKeys_UsesCorrectKey()
    {
        // Arrange
        const string customFlagKey = "custom-flag-key";
        const int defaultValue = 0;
        const int resolvedValue = 123;
        var expectedDetails = new ResolutionDetails<int>(customFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, customFlagKey, typeof(int));

        this._mockProvider.ResolveIntegerValueAsync(customFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDetails, result.ResolutionDetails);
        Assert.Equal(customFlagKey, result.ResolutionDetails.FlagKey);
        await this._mockProvider.Received(1).ResolveIntegerValueAsync(customFlagKey, defaultValue, this._evaluationContext, this._cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_WhenOperationCancelled_ReturnsErrorResult()
    {
        // Arrange
        const bool defaultValue = false;
        var cancellationTokenSource = new CancellationTokenSource();
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey, typeof(bool));

        this._mockProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, cancellationTokenSource.Token)
            .Returns(async callInfo =>
            {
                cancellationTokenSource.Cancel();
                await Task.Delay(100, cancellationTokenSource.Token);
                return new ResolutionDetails<bool>(TestFlagKey, true);
            });

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, cancellationTokenSource.Token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Equal(TestFlagKey, result.ResolutionDetails.FlagKey);
        Assert.Equal(defaultValue, result.ResolutionDetails.Value);
        Assert.Equal(ErrorType.General, result.ResolutionDetails.ErrorType);
        Assert.Equal(Reason.Error, result.ResolutionDetails.Reason);
        Assert.NotNull(result.ThrownError);
        Assert.True(result.ThrownError is OperationCanceledException);
    }

    [Fact]
    public async Task EvaluateAsync_WithComplexEvaluationContext_PassesContextToProvider()
    {
        // Arrange
        const double defaultValue = 1.0;
        const double resolvedValue = 2.5;
        var complexContext = new EvaluationContextBuilder()
            .Set("user", "test-user")
            .Set("environment", "test")
            .Build();
        var expectedDetails = new ResolutionDetails<double>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var providerContext = new StrategyPerProviderContext(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey, typeof(double));

        this._mockProvider.ResolveDoubleValueAsync(TestFlagKey, defaultValue, complexContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, complexContext, defaultValue, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDetails, result.ResolutionDetails);
        await this._mockProvider.Received(1).ResolveDoubleValueAsync(TestFlagKey, defaultValue, complexContext, this._cancellationToken);
    }
}
