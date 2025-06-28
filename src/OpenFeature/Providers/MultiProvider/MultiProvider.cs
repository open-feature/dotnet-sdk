using OpenFeature.Model;

namespace OpenFeature.Providers.MultiProvider;

/// <summary>
/// A feature provider that enables the use of multiple underlying providers, allowing different providers
/// to be used for different flag keys or based on specific routing logic.
/// </summary>
/// <remarks>
/// The MultiProvider acts as a composite provider that can delegate flag resolution to different
/// underlying providers based on configuration or routing rules. This enables scenarios where
/// different feature flags may be served by different sources or providers within the same application.
/// </remarks>
/// <seealso href="https://openfeature.dev/specification/appendix-a/#multi-provider">Multi Provider specification</seealso>
public sealed class MultiProvider : FeatureProvider
{
    private readonly Dictionary<string, FeatureProvider> _providers;
    private readonly BaseEvaluationStrategy _evaluationStrategy;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiProvider"/> class with the specified providers and evaluation strategy.
    /// </summary>
    /// <param name="providers">A dictionary containing the feature providers keyed by their identifiers.</param>
    /// <param name="evaluationStrategy">The base evaluation strategy to use for determining how to evaluate features across multiple providers.</param>
    public MultiProvider(Dictionary<string, FeatureProvider> providers, BaseEvaluationStrategy evaluationStrategy)
    {
        this._providers = providers;
        this._evaluationStrategy = evaluationStrategy;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiProvider"/> class with the specified providers. The default evaluation strategy is <see cref="FirstMatchStrategy"/>.
    /// </summary>
    /// <param name="providers">A dictionary containing the feature providers keyed by their identifiers.</param>
    public MultiProvider(Dictionary<string, FeatureProvider> providers) : this(providers, new FirstMatchStrategy())
    {
    }

    /// <inheritdoc/>
    public override Metadata GetMetadata() => new("OpenFeature MultiProvider");

    /// <inheritdoc/>
    public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        this._evaluationStrategy.EvaluateAsync(this._providers, flagKey, defaultValue, context, cancellationToken);

    /// <inheritdoc/>
    public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        this._evaluationStrategy.EvaluateAsync(this._providers, flagKey, defaultValue, context, cancellationToken);

    /// <inheritdoc/>
    public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        this._evaluationStrategy.EvaluateAsync(this._providers, flagKey, defaultValue, context, cancellationToken);

    /// <inheritdoc/>
    public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        this._evaluationStrategy.EvaluateAsync(this._providers, flagKey, defaultValue, context, cancellationToken);

    /// <inheritdoc/>
    public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        this._evaluationStrategy.EvaluateAsync(this._providers, flagKey, defaultValue, context, cancellationToken);

    /// <inheritdoc/>
    public override async Task InitializeAsync(EvaluationContext context, CancellationToken cancellationToken = default)
    {
        foreach (var provider in this._providers.Values)
        {
            await provider.InitializeAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        var exceptions = new List<Exception>();

        foreach (var provider in this._providers.Values)
        {
            try
            {
                await provider.ShutdownAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException("One or more providers failed to shutdown", exceptions);
        }
    }
}
