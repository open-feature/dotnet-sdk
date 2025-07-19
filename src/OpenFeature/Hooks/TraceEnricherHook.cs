using System.Diagnostics;
using OpenFeature.Model;
using OpenFeature.Telemetry;

namespace OpenFeature.Hooks;

/// <summary>
/// A hook that enriches telemetry traces with additional information during the feature flag evaluation lifecycle.
/// This hook adds relevant flag evaluation details as tags and events to the current <see cref="Activity"/> for tracing purposes.
/// On error, it attaches exception information to the trace, using the appropriate API depending on the .NET version.
/// </summary>
/// <remarks> This is still experimental and subject to change. </remarks>
public class TraceEnricherHook : Hook
{
    private readonly TraceEnricherHookOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraceEnricherHook"/> class.
    /// </summary>
    /// <param name="options">Optional configuration for the traces hook.</param>
    public TraceEnricherHook(TraceEnricherHookOptions? options = null)
    {
        _options = options ?? TraceEnricherHookOptions.Default;
    }

    /// <summary>
    /// Adds tags and events to the current <see cref="Activity"/> for tracing purposes.
    /// </summary>
    /// <typeparam name="T">The type of the flag value being evaluated.</typeparam>
    /// <param name="context">The hook context containing metadata about the evaluation.</param>
    /// <param name="details">Details about the flag evaluation including the key, value, and variant.</param>
    /// <param name="hints">Optional dictionary of hints that can modify hook behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A completed <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public override ValueTask FinallyAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        var evaluationEvent = EvaluationEventBuilder<T>.Build(context, details);

        var tags = new ActivityTagsCollection();
        foreach (var kvp in evaluationEvent.Attributes)
        {
            tags[kvp.Key] = kvp.Value;
        }

        this.AddCustomDimensions(tags);
        this.AddFlagMetadataDimensions(details.FlagMetadata, tags);

        Activity.Current?.AddEvent(new ActivityEvent(evaluationEvent.Name, tags: tags));

        return base.FinallyAsync(context, details, hints, cancellationToken);
    }

    private void AddCustomDimensions(ActivityTagsCollection tagList)
    {
        foreach (var customDimension in this._options.CustomDimensions)
        {
            tagList.Add(customDimension.Key, customDimension.Value);
        }
    }

    private void AddFlagMetadataDimensions(ImmutableMetadata? flagMetadata, ActivityTagsCollection tagList)
    {
        flagMetadata ??= new ImmutableMetadata();

        foreach (var item in this._options.FlagMetadataCallbacks)
        {
            var flagMetadataCallback = item.Value;
            var value = flagMetadataCallback(flagMetadata);

            tagList.Add(item.Key, value);
        }
    }
}
