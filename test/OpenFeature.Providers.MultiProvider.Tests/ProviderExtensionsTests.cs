using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Providers.MultiProvider.Tests;

public class ProviderExtensionsTests
{
    private const string TestFlagKey = "test-flag";
    private const string TestProviderName = "test-provider";
    private const string TestVariant = "test-variant";

    private readonly FeatureProvider _mockProvider = Substitute.For<FeatureProvider>();
    private readonly EvaluationContext _evaluationContext = new EvaluationContextBuilder().Build();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly ILogger _mockLogger = Substitute.For<ILogger>();

    [Fact]
    public async Task EvaluateAsync_WithBooleanType_CallsResolveBooleanValueAsync()
    {
        // Arrange
        const bool defaultValue = false;
        const bool resolvedValue = true;
        var expectedDetails = new ResolutionDetails<bool>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var providerContext = new StrategyPerProviderContext<bool>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        this._mockProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._mockLogger, this._cancellationToken);

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
        var providerContext = new StrategyPerProviderContext<string>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        this._mockProvider.ResolveStringValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._mockLogger, this._cancellationToken);

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
        var providerContext = new StrategyPerProviderContext<int>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        this._mockProvider.ResolveIntegerValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._mockLogger, this._cancellationToken);

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
        var providerContext = new StrategyPerProviderContext<double>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        this._mockProvider.ResolveDoubleValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._mockLogger, this._cancellationToken);

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
        var providerContext = new StrategyPerProviderContext<Value>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        this._mockProvider.ResolveStructureValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._mockLogger, this._cancellationToken);

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
        var providerContext = new StrategyPerProviderContext<DateTime>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._mockLogger, this._cancellationToken);

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
        var providerContext = new StrategyPerProviderContext<bool>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        this._mockProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .ThrowsAsync(expectedException);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._mockLogger, this._cancellationToken);

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
        var providerContext = new StrategyPerProviderContext<bool>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        this._mockProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, null, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, null, defaultValue, this._mockLogger, this._cancellationToken);

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
        var providerContext = new StrategyPerProviderContext<string>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);
        var customCancellationToken = new CancellationTokenSource().Token;

        this._mockProvider.ResolveStringValueAsync(TestFlagKey, defaultValue, this._evaluationContext, customCancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._mockLogger, customCancellationToken);

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
        var providerContext = new StrategyPerProviderContext<string>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        this._mockProvider.ResolveStringValueAsync(TestFlagKey, defaultValue!, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue!, this._mockLogger, this._cancellationToken);

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
        var providerContext = new StrategyPerProviderContext<int>(this._mockProvider, TestProviderName, ProviderStatus.Ready, customFlagKey);

        this._mockProvider.ResolveIntegerValueAsync(customFlagKey, defaultValue, this._evaluationContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._mockLogger, this._cancellationToken);

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
        var providerContext = new StrategyPerProviderContext<bool>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        this._mockProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, cancellationTokenSource.Token)
            .Returns(async _ =>
            {
                // net462 does not support CancellationTokenSource.CancelAfter
                // ReSharper disable once MethodHasAsyncOverload
                cancellationTokenSource.Cancel();
                await Task.Delay(100, cancellationTokenSource.Token);
                return new ResolutionDetails<bool>(TestFlagKey, true);
            });

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._mockLogger, cancellationTokenSource.Token);

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
        var providerContext = new StrategyPerProviderContext<double>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        this._mockProvider.ResolveDoubleValueAsync(TestFlagKey, defaultValue, complexContext, this._cancellationToken)
            .Returns(expectedDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, complexContext, defaultValue, this._mockLogger, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDetails, result.ResolutionDetails);
        await this._mockProvider.Received(1).ResolveDoubleValueAsync(TestFlagKey, defaultValue, complexContext, this._cancellationToken);
    }

    [Fact]
    public async Task EvaluateAsync_WithProviderHooksAndErrorResult_TriggersErrorHooks()
    {
        // Arrange
        var mockHook = Substitute.For<Hook>();

        // Setup hook to return evaluation context successfully
        mockHook.BeforeAsync(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<CancellationToken>())
            .Returns(EvaluationContext.Empty);

        // Setup provider metadata
        var providerMetadata = new Metadata(TestProviderName);
        this._mockProvider.GetMetadata().Returns(providerMetadata);
        this._mockProvider.GetProviderHooks().Returns(ImmutableList.Create(mockHook));

        const bool defaultValue = false;
        var errorDetails = new ResolutionDetails<bool>(
            TestFlagKey,
            defaultValue,
            ErrorType.FlagNotFound,
            Reason.Error,
            TestVariant,
            errorMessage: "Flag not found");

        var providerContext = new StrategyPerProviderContext<bool>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        this._mockProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, Arg.Any<EvaluationContext>(), this._cancellationToken)
            .Returns(errorDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._mockLogger, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ErrorType.FlagNotFound, result.ResolutionDetails.ErrorType);
        Assert.Equal(Reason.Error, result.ResolutionDetails.Reason);

        // Verify before hook was called
        await mockHook.Received(1).BeforeAsync(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<CancellationToken>());

        // Verify error hook was called (not after hook)
        await mockHook.Received(1).ErrorAsync(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<CancellationToken>());
        await mockHook.DidNotReceive().AfterAsync(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<CancellationToken>());

        // Verify finally hook was called
        await mockHook.Received(1).FinallyAsync(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(typeof(bool), FlagValueType.Boolean)]
    [InlineData(typeof(string), FlagValueType.String)]
    [InlineData(typeof(int), FlagValueType.Number)]
    [InlineData(typeof(double), FlagValueType.Number)]
    [InlineData(typeof(Value), FlagValueType.Object)]
    [InlineData(typeof(ProviderExtensionsTests), FlagValueType.Object)] // fallback path
    public void GetFlagValueType_ReturnsExpectedFlagValueType(Type inputType, FlagValueType expected)
    {
        FlagValueType result = inputType == typeof(bool) ? ProviderExtensions.GetFlagValueType<bool>()
            : inputType == typeof(string) ? ProviderExtensions.GetFlagValueType<string>()
            : inputType == typeof(int) ? ProviderExtensions.GetFlagValueType<int>()
            : inputType == typeof(double) ? ProviderExtensions.GetFlagValueType<double>()
            : inputType == typeof(Value) ? ProviderExtensions.GetFlagValueType<Value>()
            : ProviderExtensions.GetFlagValueType<ProviderExtensionsTests>();

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenAfterHookThrowsException_LogsWarningButSucceeds()
    {
        // Arrange
        var hookException = new InvalidOperationException("After hook failed");
        var throwingHook = new ThrowingAfterHook(hookException);

        // Setup provider metadata and hooks
        var providerMetadata = new Metadata(TestProviderName);
        this._mockProvider.GetMetadata().Returns(providerMetadata);
        this._mockProvider.GetProviderHooks().Returns(ImmutableList.Create<Hook>(throwingHook));

        const bool defaultValue = false;
        const bool resolvedValue = true;
        var successDetails = new ResolutionDetails<bool>(
            TestFlagKey,
            resolvedValue,
            ErrorType.None,
            Reason.Static,
            TestVariant);

        var providerContext = new StrategyPerProviderContext<bool>(this._mockProvider, TestProviderName, ProviderStatus.Ready, TestFlagKey);

        this._mockProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, Arg.Any<EvaluationContext>(), this._cancellationToken)
            .Returns(successDetails);

        // Act
        var result = await this._mockProvider.EvaluateAsync(providerContext, this._evaluationContext, defaultValue, this._mockLogger, this._cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resolvedValue, result.ResolutionDetails.Value);
        Assert.Equal(ErrorType.None, result.ResolutionDetails.ErrorType);
        Assert.Null(result.ThrownError); // Hook errors don't propagate

        // Verify warning was logged
        this._mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Provider after/finally hook execution failed")),
            Arg.Is<Exception>(ex => ex == hookException),
            Arg.Any<Func<object, Exception?, string>>());
    }
}

internal class ThrowingAfterHook : Hook
{
    private InvalidOperationException hookException;

    public ThrowingAfterHook(InvalidOperationException hookException)
    {
        this.hookException = hookException;
    }

    public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        throw this.hookException;
    }
}
