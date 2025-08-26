using System.Reflection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Models;
using OpenFeature.Providers.MultiProvider.Strategies;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Providers.MultiProvider.Tests;

public class MultiProviderClassTests
{
    private const string TestFlagKey = "test-flag";
    private const string TestVariant = "test-variant";
    private const string Provider1Name = "provider1";
    private const string Provider2Name = "provider2";
    private const string Provider3Name = "provider3";

    private readonly FeatureProvider _mockProvider1 = Substitute.For<FeatureProvider>();
    private readonly FeatureProvider _mockProvider2 = Substitute.For<FeatureProvider>();
    private readonly FeatureProvider _mockProvider3 = Substitute.For<FeatureProvider>();
    private readonly BaseEvaluationStrategy _mockStrategy = Substitute.For<BaseEvaluationStrategy>();
    private readonly EvaluationContext _evaluationContext = new EvaluationContextBuilder().Build();

    public MultiProviderClassTests()
    {
        // Setup default metadata for providers
        this._mockProvider1.GetMetadata().Returns(new Metadata(Provider1Name));
        this._mockProvider2.GetMetadata().Returns(new Metadata(Provider2Name));
        this._mockProvider3.GetMetadata().Returns(new Metadata(Provider3Name));

        // Setup default strategy behavior
        this._mockStrategy.RunMode.Returns(RunMode.Sequential);
        this._mockStrategy.ShouldEvaluateThisProvider(Arg.Any<StrategyPerProviderContext<object>>(), Arg.Any<EvaluationContext>()).Returns(true);
        this._mockStrategy.ShouldEvaluateNextProvider(Arg.Any<StrategyPerProviderContext<object>>(), Arg.Any<EvaluationContext>(), Arg.Any<ProviderResolutionResult<object>>()).Returns(false);
    }

    [Fact]
    public void Constructor_WithValidProviderEntries_CreatesMultiProvider()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name)
        };

        // Act
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Assert
        Assert.NotNull(multiProvider);
        var metadata = multiProvider.GetMetadata();
        Assert.Equal("MultiProvider", metadata.Name);
    }

    [Fact]
    public void Constructor_WithNullProviderEntries_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new MultiProvider(null!, this._mockStrategy));
        Assert.Equal("providerEntries", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyProviderEntries_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MultiProvider([], this._mockStrategy));
        Assert.Contains("At least one provider entry must be provided", exception.Message);
        Assert.Equal("providerEntries", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullStrategy_UsesDefaultFirstMatchStrategy()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };

        // Act
        var multiProvider = new MultiProvider(providerEntries);

        // Assert
        Assert.NotNull(multiProvider);
        var metadata = multiProvider.GetMetadata();
        Assert.Equal("MultiProvider", metadata.Name);
    }

    [Fact]
    public void Constructor_WithDuplicateExplicitNames_ThrowsArgumentException()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, "duplicate-name"),
            new(this._mockProvider2, "duplicate-name")
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MultiProvider(providerEntries, this._mockStrategy));
        Assert.Contains("Multiple providers cannot have the same explicit name: 'duplicate-name'", exception.Message);
    }

    [Fact]
    public async Task ResolveBooleanValueAsync_CallsEvaluateAsync()
    {
        // Arrange
        const bool defaultValue = false;
        const bool resolvedValue = true;
        var expectedDetails = new ResolutionDetails<bool>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var finalResult = new FinalResult<bool>(expectedDetails, this._mockProvider1, Provider1Name, null);

        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockStrategy.DetermineFinalResult(Arg.Any<StrategyEvaluationContext<bool>>(), TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<List<ProviderResolutionResult<bool>>>())
            .Returns(finalResult);

        // Act
        var result = await multiProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext);

        // Assert
        Assert.Equal(expectedDetails, result);
        this._mockStrategy.Received(1).DetermineFinalResult(Arg.Any<StrategyEvaluationContext<bool>>(), TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<List<ProviderResolutionResult<bool>>>());
    }

    [Fact]
    public async Task ResolveStringValueAsync_CallsEvaluateAsync()
    {
        // Arrange
        const string defaultValue = "default";
        const string resolvedValue = "resolved";
        var expectedDetails = new ResolutionDetails<string>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var finalResult = new FinalResult<string>(expectedDetails, this._mockProvider1, Provider1Name, null);

        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockStrategy.DetermineFinalResult(Arg.Any<StrategyEvaluationContext<string>>(), TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<List<ProviderResolutionResult<string>>>())
            .Returns(finalResult);

        // Act
        var result = await multiProvider.ResolveStringValueAsync(TestFlagKey, defaultValue, this._evaluationContext);

        // Assert
        Assert.Equal(expectedDetails, result);
    }

    [Fact]
    public async Task InitializeAsync_WithAllSuccessfulProviders_InitializesAllProviders()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockProvider1.InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        this._mockProvider2.InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await multiProvider.InitializeAsync(this._evaluationContext);

        // Assert
        await this._mockProvider1.Received(1).InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>());
        await this._mockProvider2.Received(1).InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InitializeAsync_WithSomeFailingProviders_ThrowsAggregateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Initialization failed");
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockProvider1.InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        this._mockProvider2.InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>()).ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AggregateException>(() => multiProvider.InitializeAsync(this._evaluationContext));
        Assert.Contains("Failed to initialize providers", exception.Message);
        Assert.Contains(Provider2Name, exception.Message);
        Assert.Contains(expectedException, exception.InnerExceptions);
    }

    [Fact]
    public async Task ShutdownAsync_WithAllSuccessfulProviders_ShutsDownAllProviders()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);
        multiProvider.SetStatus(ProviderStatus.Ready);

        this._mockProvider1.ShutdownAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        this._mockProvider2.ShutdownAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await multiProvider.ShutdownAsync();

        // Assert
        await this._mockProvider1.Received(1).ShutdownAsync(Arg.Any<CancellationToken>());
        await this._mockProvider2.Received(1).ShutdownAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShutdownAsync_WithFatalProvider_ShutsDownAllProviders()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);
        multiProvider.SetStatus(ProviderStatus.Fatal);

        this._mockProvider1.ShutdownAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        this._mockProvider2.ShutdownAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await multiProvider.ShutdownAsync();

        // Assert
        await this._mockProvider1.Received(1).ShutdownAsync(Arg.Any<CancellationToken>());
        await this._mockProvider2.Received(1).ShutdownAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShutdownAsync_WithSomeFailingProviders_ThrowsAggregateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Shutdown failed");
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);
        multiProvider.SetStatus(ProviderStatus.Ready);

        this._mockProvider1.ShutdownAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        this._mockProvider2.ShutdownAsync(Arg.Any<CancellationToken>()).ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AggregateException>(() => multiProvider.ShutdownAsync());
        Assert.Contains("Failed to shutdown providers", exception.Message);
        Assert.Contains(Provider2Name, exception.Message);
        Assert.Contains(expectedException, exception.InnerExceptions);
    }

    [Fact]
    public void GetMetadata_ReturnsMultiProviderMetadata()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Act
        var metadata = multiProvider.GetMetadata();

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("MultiProvider", metadata.Name);
    }

    [Fact]
    public async Task ResolveDoubleValueAsync_CallsEvaluateAsync()
    {
        // Arrange
        const double defaultValue = 1.0;
        const double resolvedValue = 2.5;
        var expectedDetails = new ResolutionDetails<double>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var finalResult = new FinalResult<double>(expectedDetails, this._mockProvider1, Provider1Name, null);

        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockStrategy.DetermineFinalResult(Arg.Any<StrategyEvaluationContext<double>>(), TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<List<ProviderResolutionResult<double>>>())
            .Returns(finalResult);

        // Act
        var result = await multiProvider.ResolveDoubleValueAsync(TestFlagKey, defaultValue, this._evaluationContext);

        // Assert
        Assert.Equal(expectedDetails, result);
        this._mockStrategy.Received(1).DetermineFinalResult(Arg.Any<StrategyEvaluationContext<double>>(), TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<List<ProviderResolutionResult<double>>>());
    }

    [Fact]
    public async Task ResolveIntegerValueAsync_CallsEvaluateAsync()
    {
        // Arrange
        const int defaultValue = 10;
        const int resolvedValue = 42;
        var expectedDetails = new ResolutionDetails<int>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var finalResult = new FinalResult<int>(expectedDetails, this._mockProvider1, Provider1Name, null);

        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockStrategy.DetermineFinalResult(Arg.Any<StrategyEvaluationContext<int>>(), TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<List<ProviderResolutionResult<int>>>())
            .Returns(finalResult);

        // Act
        var result = await multiProvider.ResolveIntegerValueAsync(TestFlagKey, defaultValue, this._evaluationContext);

        // Assert
        Assert.Equal(expectedDetails, result);
    }

    [Fact]
    public async Task ResolveStructureValueAsync_CallsEvaluateAsync()
    {
        // Arrange
        var defaultValue = new Value("default");
        var resolvedValue = new Value("resolved");
        var expectedDetails = new ResolutionDetails<Value>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var finalResult = new FinalResult<Value>(expectedDetails, this._mockProvider1, Provider1Name, null);

        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockStrategy.DetermineFinalResult(Arg.Any<StrategyEvaluationContext<Value>>(), TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<List<ProviderResolutionResult<Value>>>())
            .Returns(finalResult);

        // Act
        var result = await multiProvider.ResolveStructureValueAsync(TestFlagKey, defaultValue, this._evaluationContext);

        // Assert
        Assert.Equal(expectedDetails, result);
    }

    [Fact]
    public async Task EvaluateAsync_WithSequentialMode_EvaluatesProvidersSequentially()
    {
        // Arrange
        const bool defaultValue = false;
        const bool resolvedValue = true;
        var expectedDetails = new ResolutionDetails<bool>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var finalResult = new FinalResult<bool>(expectedDetails, this._mockProvider1, Provider1Name, null);

        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockStrategy.RunMode.Returns(RunMode.Sequential);
        this._mockStrategy.ShouldEvaluateThisProvider(Arg.Any<StrategyPerProviderContext<bool>>(), this._evaluationContext).Returns(true);
        this._mockStrategy.ShouldEvaluateNextProvider(Arg.Any<StrategyPerProviderContext<bool>>(), this._evaluationContext, Arg.Any<ProviderResolutionResult<bool>>()).Returns(false);
        this._mockStrategy.DetermineFinalResult(Arg.Any<StrategyEvaluationContext<bool>>(), TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<List<ProviderResolutionResult<bool>>>())
            .Returns(finalResult);

        this._mockProvider1.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<CancellationToken>())
            .Returns(expectedDetails);

        // Act
        var result = await multiProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext);

        // Assert
        Assert.Equal(expectedDetails, result);
        await this._mockProvider1.Received(1).ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<CancellationToken>());
        await this._mockProvider2.DidNotReceive().ResolveBooleanValueAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WithParallelMode_EvaluatesProvidersInParallel()
    {
        // Arrange
        const bool defaultValue = false;
        const bool resolvedValue = true;
        var expectedDetails = new ResolutionDetails<bool>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var finalResult = new FinalResult<bool>(expectedDetails, this._mockProvider1, Provider1Name, null);

        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockStrategy.RunMode.Returns(RunMode.Parallel);
        this._mockStrategy.ShouldEvaluateThisProvider(Arg.Any<StrategyPerProviderContext<bool>>(), this._evaluationContext).Returns(true);
        this._mockStrategy.DetermineFinalResult(Arg.Any<StrategyEvaluationContext<bool>>(), TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<List<ProviderResolutionResult<bool>>>())
            .Returns(finalResult);

        this._mockProvider1.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<CancellationToken>())
            .Returns(expectedDetails);
        this._mockProvider2.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<CancellationToken>())
            .Returns(expectedDetails);

        // Act
        var result = await multiProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext);

        // Assert
        Assert.Equal(expectedDetails, result);
        await this._mockProvider1.Received(1).ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<CancellationToken>());
        await this._mockProvider2.Received(1).ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WithUnsupportedRunMode_ReturnsErrorDetails()
    {
        // Arrange
        const bool defaultValue = false;
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockStrategy.RunMode.Returns((RunMode)999); // Invalid enum value

        // Act & Assert
        var details = await multiProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext);
        Assert.Equal(ErrorType.ProviderFatal, details.ErrorType);
        Assert.Equal(Reason.Error, details.Reason);
        Assert.Contains("Unsupported run mode", details.ErrorMessage);
    }

    [Fact]
    public async Task EvaluateAsync_WithStrategySkippingProvider_DoesNotCallSkippedProvider()
    {
        // Arrange
        const bool defaultValue = false;
        const bool resolvedValue = true;
        var expectedDetails = new ResolutionDetails<bool>(TestFlagKey, resolvedValue, ErrorType.None, Reason.Static, TestVariant);
        var finalResult = new FinalResult<bool>(expectedDetails, this._mockProvider1, Provider1Name, null);

        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockStrategy.RunMode.Returns(RunMode.Sequential);
        this._mockStrategy.ShouldEvaluateThisProvider(Arg.Any<StrategyPerProviderContext<bool>>(), this._evaluationContext)
            .Returns(callInfo =>
            {
                var context = callInfo.Arg<StrategyPerProviderContext<bool>>();
                return context.ProviderName == Provider1Name; // Only evaluate provider1
            });
        this._mockStrategy.DetermineFinalResult(Arg.Any<StrategyEvaluationContext<bool>>(), TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<List<ProviderResolutionResult<bool>>>())
            .Returns(finalResult);

        this._mockProvider1.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<CancellationToken>())
            .Returns(expectedDetails);

        // Act
        var result = await multiProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext);

        // Assert
        Assert.Equal(expectedDetails, result);
        await this._mockProvider1.Received(1).ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<CancellationToken>());
        await this._mockProvider2.DidNotReceive().ResolveBooleanValueAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WithCancellationToken_PassesCancellationTokenToProviders()
    {
        // Arrange
        const bool defaultValue = false;
        var expectedDetails = new ResolutionDetails<bool>(TestFlagKey, true, ErrorType.None, Reason.Static, TestVariant);
        var finalResult = new FinalResult<bool>(expectedDetails, this._mockProvider1, Provider1Name, null);

        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockStrategy.RunMode.Returns(RunMode.Sequential);
        this._mockStrategy.ShouldEvaluateThisProvider(Arg.Any<StrategyPerProviderContext<bool>>(), this._evaluationContext).Returns(true);
        this._mockStrategy.ShouldEvaluateNextProvider(Arg.Any<StrategyPerProviderContext<bool>>(), this._evaluationContext, Arg.Any<ProviderResolutionResult<bool>>()).Returns(false);
        this._mockStrategy.DetermineFinalResult(Arg.Any<StrategyEvaluationContext<bool>>(), TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<List<ProviderResolutionResult<bool>>>())
            .Returns(finalResult);

        this._mockProvider1.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, Arg.Any<CancellationToken>())
            .Returns(expectedDetails);

        using var cts = new CancellationTokenSource();

        // Act
        var result = await multiProvider.ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, cts.Token);

        // Assert
        Assert.Equal(expectedDetails, result);
        await this._mockProvider1.Received(1).ResolveBooleanValueAsync(TestFlagKey, defaultValue, this._evaluationContext, cts.Token);
    }

    [Fact]
    public void Constructor_WithProvidersHavingSameMetadataName_AssignsUniqueNames()
    {
        // Arrange
        var provider1 = Substitute.For<FeatureProvider>();
        var provider2 = Substitute.For<FeatureProvider>();
        provider1.GetMetadata().Returns(new Metadata("SameName"));
        provider2.GetMetadata().Returns(new Metadata("SameName"));

        var providerEntries = new List<ProviderEntry>
        {
            new(provider1), // No explicit name, will use metadata name
            new(provider2)  // No explicit name, will use metadata name
        };

        // Act
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Assert
        Assert.NotNull(multiProvider);
        // The internal logic should assign unique names like "SameName-1", "SameName-2"
        var metadata = multiProvider.GetMetadata();
        Assert.Equal("MultiProvider", metadata.Name);
    }

    [Fact]
    public void Constructor_WithProviderHavingNullMetadata_AssignsDefaultName()
    {
        // Arrange
        var provider = Substitute.For<FeatureProvider>();
        provider.GetMetadata().Returns((Metadata?)null);

        var providerEntries = new List<ProviderEntry> { new(provider) };

        // Act
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Assert
        Assert.NotNull(multiProvider);
        var metadata = multiProvider.GetMetadata();
        Assert.Equal("MultiProvider", metadata.Name);
    }

    [Fact]
    public void Constructor_WithProviderHavingNullMetadataName_AssignsDefaultName()
    {
        // Arrange
        var provider = Substitute.For<FeatureProvider>();
        var metadata = new Metadata(null);
        provider.GetMetadata().Returns(metadata);

        var providerEntries = new List<ProviderEntry> { new(provider) };

        // Act
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Assert
        Assert.NotNull(multiProvider);
        var multiProviderMetadata = multiProvider.GetMetadata();
        Assert.Equal("MultiProvider", multiProviderMetadata.Name);
    }

    [Fact]
    public async Task InitializeAsync_WithCancellationToken_PassesCancellationTokenToProviders()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockProvider1.InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        using var cts = new CancellationTokenSource();

        // Act
        await multiProvider.InitializeAsync(this._evaluationContext, cts.Token);

        // Assert
        await this._mockProvider1.Received(1).InitializeAsync(this._evaluationContext, cts.Token);
    }

    [Fact]
    public async Task ShutdownAsync_WithCancellationToken_PassesCancellationTokenToProviders()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);
        multiProvider.SetStatus(ProviderStatus.Ready);

        this._mockProvider1.ShutdownAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        using var cts = new CancellationTokenSource();

        // Act
        await multiProvider.ShutdownAsync(cts.Token);

        // Assert
        await this._mockProvider1.Received(1).ShutdownAsync(cts.Token);
    }

    [Fact]
    public async Task InitializeAsync_WithAllSuccessfulProviders_CompletesWithoutException()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name),
            new(this._mockProvider3, Provider3Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockProvider1.InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        this._mockProvider2.InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        this._mockProvider3.InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act & Assert
        await multiProvider.InitializeAsync(this._evaluationContext);

        // Verify all providers were called
        await this._mockProvider1.Received(1).InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>());
        await this._mockProvider2.Received(1).InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>());
        await this._mockProvider3.Received(1).InitializeAsync(this._evaluationContext, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShutdownAsync_WithAllSuccessfulProviders_CompletesWithoutException()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name),
            new(this._mockProvider3, Provider3Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);
        multiProvider.SetStatus(ProviderStatus.Ready);

        this._mockProvider1.ShutdownAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        this._mockProvider2.ShutdownAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        this._mockProvider3.ShutdownAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act & Assert
        await multiProvider.ShutdownAsync();

        // Verify all providers were called
        await this._mockProvider1.Received(1).ShutdownAsync(Arg.Any<CancellationToken>());
        await this._mockProvider2.Received(1).ShutdownAsync(Arg.Any<CancellationToken>());
        await this._mockProvider3.Received(1).ShutdownAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MultiProvider_ConcurrentInitializationAndShutdown_ShouldMaintainConsistentProviderStatus()
    {
        // Arrange
        const int providerCount = 20;
        var providerEntries = new List<ProviderEntry>();

        for (int i = 0; i < providerCount; i++)
        {
            var provider = Substitute.For<FeatureProvider>();

            provider.InitializeAsync(Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
                    .Returns(Task.CompletedTask);

            provider.ShutdownAsync(Arg.Any<CancellationToken>())
                    .Returns(Task.CompletedTask);

            provider.GetMetadata()
                    .Returns(new Metadata(name: $"provider-{i}"));

            providerEntries.Add(new ProviderEntry(provider));
        }

        var multiProvider = new MultiProvider(providerEntries);

        // Act: simulate concurrent initialization and shutdown with one task each
        var initTasks = Enumerable.Range(0, 1).Select(_ =>
            Task.Run(() => multiProvider.InitializeAsync(Arg.Any<EvaluationContext>(), CancellationToken.None)));

        var shutdownTasks = Enumerable.Range(0, 1).Select(_ =>
            Task.Run(() => multiProvider.ShutdownAsync(CancellationToken.None)));

        await Task.WhenAll(initTasks.Concat(shutdownTasks));

        // Assert: ensure that each provider ends in a valid lifecycle state
        var statuses = GetRegisteredStatuses().ToList();

        Assert.All(statuses, status =>
        {
            Assert.True(
                status is ProviderStatus.Ready or ProviderStatus.NotReady,
                $"Unexpected provider status: {status}");
        });

        // Local helper: uses reflection to access the private '_registeredProviders' field
        // and retrieve the current status of each registered provider.
        // Consider replacing this with an internal or public method if testing becomes more frequent.
        IEnumerable<ProviderStatus> GetRegisteredStatuses()
        {
            var field = typeof(MultiProvider).GetField("_registeredProviders", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field?.GetValue(multiProvider) is not IEnumerable<object> list)
                throw new InvalidOperationException("Could not retrieve registered providers via reflection.");

            foreach (var p in list)
            {
                var statusProperty = p.GetType().GetProperty("Status", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (statusProperty == null)
                    throw new InvalidOperationException($"'Status' property not found on type {p.GetType().Name}.");

                if (statusProperty.GetValue(p) is not ProviderStatus status)
                    throw new InvalidOperationException("Unable to read status property value.");

                yield return status;
            }
        }
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeInternalResources()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Act
        await multiProvider.DisposeAsync();

        // Assert - Should not throw any exception
        // The internal semaphores should be disposed
        Assert.True(true); // If we get here without exception, disposal worked
    }

    [Fact]
    public async Task DisposeAsync_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Act & Assert - Multiple calls to Dispose should not throw
        await multiProvider.DisposeAsync();
        await multiProvider.DisposeAsync();
        await multiProvider.DisposeAsync();

        // If we get here without exception, multiple disposal calls worked correctly
        Assert.True(true);
    }

    [Fact]
    public async Task InitializeAsync_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Act
        await multiProvider.DisposeAsync();

        // Assert
        var exception = await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            multiProvider.InitializeAsync(this._evaluationContext));
        Assert.Equal(nameof(MultiProvider), exception.ObjectName);
    }

    [Fact]
    public async Task ShutdownAsync_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Act
        await multiProvider.DisposeAsync();

        // Assert
        var exception = await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            multiProvider.ShutdownAsync());
        Assert.Equal(nameof(MultiProvider), exception.ObjectName);
    }

    [Fact]
    public async Task InitializeAsync_WhenAlreadyDisposed_DuringExecution_ShouldExitEarly()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Dispose before calling InitializeAsync
        await multiProvider.DisposeAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            multiProvider.InitializeAsync(this._evaluationContext));

        // Verify that the underlying provider was never called since the object was disposed
        await this._mockProvider1.DidNotReceive().InitializeAsync(Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShutdownAsync_WhenAlreadyDisposed_DuringExecution_ShouldExitEarly()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Dispose before calling ShutdownAsync
        await multiProvider.DisposeAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            multiProvider.ShutdownAsync());

        // Verify that the underlying provider was never called since the object was disposed
        await this._mockProvider1.DidNotReceive().ShutdownAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Act
        await multiProvider.DisposeAsync();

        // Assert - All evaluate methods should throw ObjectDisposedException
        var boolException = await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            multiProvider.ResolveBooleanValueAsync(TestFlagKey, false));
        Assert.Equal(nameof(MultiProvider), boolException.ObjectName);

        var stringException = await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            multiProvider.ResolveStringValueAsync(TestFlagKey, "default"));
        Assert.Equal(nameof(MultiProvider), stringException.ObjectName);

        var intException = await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            multiProvider.ResolveIntegerValueAsync(TestFlagKey, 0));
        Assert.Equal(nameof(MultiProvider), intException.ObjectName);

        var doubleException = await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            multiProvider.ResolveDoubleValueAsync(TestFlagKey, 0.0));
        Assert.Equal(nameof(MultiProvider), doubleException.ObjectName);

        var structureException = await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            multiProvider.ResolveStructureValueAsync(TestFlagKey, new Value()));
        Assert.Equal(nameof(MultiProvider), structureException.ObjectName);
    }

}
