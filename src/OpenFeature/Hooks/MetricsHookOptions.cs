namespace OpenFeature.Hooks;

/// <summary>
/// Configuration options for the <see cref="MetricsHook"/>.
/// </summary>
public sealed class MetricsHookOptions
{
    /// <summary>
    /// The default options for the <see cref="MetricsHook"/>.
    /// </summary>
    public static MetricsHookOptions Default { get; } = new MetricsHookOptions();

    /// <summary>
    /// Custom dimensions or tags to be associated with Meters in <see cref="MetricsHook"/>.
    /// </summary>
    public IReadOnlyCollection<KeyValuePair<string, object?>> CustomDimensions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsHookOptions"/> class.
    /// </summary>
    /// <param name="customDimensions">Optional custom dimensions to tag Counter increments with.</param>
    public MetricsHookOptions(IReadOnlyCollection<KeyValuePair<string, object?>>? customDimensions = null)
    {
        this.CustomDimensions = customDimensions ?? [];
    }

    /// <summary>
    /// Creates a new builder for <see cref="MetricsHookOptions"/>.
    /// </summary>
    public static MetricsHookOptionsBuilder CreateBuilder() => new MetricsHookOptionsBuilder();

    /// <summary>
    /// A builder for constructing <see cref="MetricsHookOptions"/> instances.
    /// </summary>
    public sealed class MetricsHookOptionsBuilder
    {
        private readonly List<KeyValuePair<string, object?>> _customDimensions = new List<KeyValuePair<string, object?>>();

        /// <summary>
        /// Adds a custom dimension.
        /// </summary>
        /// <param name="key">The key for the custom dimension.</param>
        /// <param name="value">The value for the custom dimension.</param>
        public MetricsHookOptionsBuilder WithCustomDimension(string key, object? value)
        {
            this._customDimensions.Add(new KeyValuePair<string, object?>(key, value));
            return this;
        }

        /// <summary>
        /// Builds the <see cref="MetricsHookOptions"/> instance.
        /// </summary>
        public MetricsHookOptions Build()
        {
            return new MetricsHookOptions(this._customDimensions.AsReadOnly());
        }
    }
}
