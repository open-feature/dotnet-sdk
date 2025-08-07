using OpenFeature.Model;

namespace OpenFeature.Hooks;

/// <summary>
/// Configuration options for the <see cref="TraceEnricherHookOptions"/>.
/// </summary>
public sealed class TraceEnricherHookOptions
{
    /// <summary>
    /// The default options for the <see cref="TraceEnricherHookOptions"/>.
    /// </summary>
    public static TraceEnricherHookOptions Default { get; } = new TraceEnricherHookOptions();

    /// <summary>
    /// Custom tags to be associated with current <see cref="System.Diagnostics.Activity"/> in <see cref="TraceEnricherHook"/>.
    /// </summary>
    public IReadOnlyCollection<KeyValuePair<string, object?>> Tags { get; }

    /// <summary>
    /// Flag metadata callbacks to be associated with current <see cref="System.Diagnostics.Activity"/>.
    /// </summary>
    internal IReadOnlyCollection<KeyValuePair<string, Func<ImmutableMetadata, object?>>> FlagMetadataCallbacks { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TraceEnricherHookOptions"/> class with default values.
    /// </summary>
    private TraceEnricherHookOptions() : this(null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TraceEnricherHookOptions"/> class.
    /// </summary>
    /// <param name="tags">Optional custom tags to tag Counter increments with.</param>
    /// <param name="flagMetadataSelectors">Optional flag metadata callbacks to be associated with current <see cref="System.Diagnostics.Activity"/>.</param>
    internal TraceEnricherHookOptions(IReadOnlyCollection<KeyValuePair<string, object?>>? tags = null,
        IReadOnlyCollection<KeyValuePair<string, Func<ImmutableMetadata, object?>>>? flagMetadataSelectors = null)
    {
        this.Tags = tags ?? [];
        this.FlagMetadataCallbacks = flagMetadataSelectors ?? [];
    }

    /// <summary>
    /// Creates a new builder for <see cref="TraceEnricherHookOptions"/>.
    /// </summary>
    public static TraceEnricherHookOptionsBuilder CreateBuilder() => new TraceEnricherHookOptionsBuilder();

    /// <summary>
    /// A builder for constructing <see cref="TraceEnricherHookOptions"/> instances.
    /// </summary>
    public sealed class TraceEnricherHookOptionsBuilder
    {
        private readonly List<KeyValuePair<string, object?>> _customTags = new List<KeyValuePair<string, object?>>();
        private readonly List<KeyValuePair<string, Func<ImmutableMetadata, object?>>> _flagMetadataExpressions = new List<KeyValuePair<string, Func<ImmutableMetadata, object?>>>();

        /// <summary>
        /// Adds a custom tag to the <see cref="TraceEnricherHookOptionsBuilder"/>.
        /// </summary>
        /// <param name="key">The key for the custom dimension.</param>
        /// <param name="value">The value for the custom dimension.</param>
        public TraceEnricherHookOptionsBuilder WithTag(string key, object? value)
        {
            this._customTags.Add(new KeyValuePair<string, object?>(key, value));
            return this;
        }

        /// <summary>
        /// Provide a callback to evaluate flag metadata and add it as a custom tag on the current <see cref="System.Diagnostics.Activity"/>.
        /// </summary>
        /// <param name="key">The key for the custom tag.</param>
        /// <param name="flagMetadataCallback">The callback to retrieve the value to tag successful flag evaluations.</param>
        /// <returns></returns>
        public TraceEnricherHookOptionsBuilder WithFlagEvaluationMetadata(string key, Func<ImmutableMetadata, object?> flagMetadataCallback)
        {
            var kvp = new KeyValuePair<string, Func<ImmutableMetadata, object?>>(key, flagMetadataCallback);

            this._flagMetadataExpressions.Add(kvp);

            return this;
        }

        /// <summary>
        /// Builds the <see cref="TraceEnricherHookOptions"/> instance.
        /// </summary>
        public TraceEnricherHookOptions Build()
        {
            return new TraceEnricherHookOptions(this._customTags.AsReadOnly(), this._flagMetadataExpressions.AsReadOnly());
        }
    }
}
