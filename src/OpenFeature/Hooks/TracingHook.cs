using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OpenFeature.Model;
using OpenFeature.Telemetry;

namespace OpenFeature.Hooks;

/// <summary>
/// A hook that enriches telemetry traces with additional information during the feature flag evaluation lifecycle.
/// </summary>
/// <remarks>
/// This hook adds relevant flag evaluation details as tags and events to the current <see cref="Activity"/> for tracing purposes.
/// On error, it attaches exception information to the trace, using the appropriate API depending on the .NET version.
/// </remarks>
public class TracingHook : Hook
{
    /// <summary>
    /// Adds tags and events to the current <see cref="Activity"/> for tracing purposes.
    /// </summary>
    /// <typeparam name="T">The type of the flag value being evaluated.</typeparam>
    /// <param name="context">The hook context containing metadata about the evaluation.</param>
    /// <param name="details">Details about the flag evaluation including the key, value, and variant.</param>
    /// <param name="hints">Optional dictionary of hints that can modify hook behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A completed <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details,
        IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        Activity.Current?
            .SetTag(TelemetryConstants.Key, details.FlagKey)
            .SetTag(TelemetryConstants.Variant, details.Variant)
            .SetTag(TelemetryConstants.Provider, context.ProviderMetadata.Name)
            .AddEvent(new ActivityEvent("feature_flag", tags: new ActivityTagsCollection
            {
                [TelemetryConstants.Key] = details.FlagKey,
                [TelemetryConstants.Variant] = details.Variant,
                [TelemetryConstants.Provider] = context.ProviderMetadata.Name
            }));

        return default;
    }

    /// <summary>
    /// Records exception information to the current <see cref="Activity"/> when a feature flag evaluation encounters an error.
    /// </summary>
    /// <typeparam name="T">The type of the flag value being evaluated.</typeparam>
    /// <param name="context">The hook context containing metadata about the evaluation.</param>
    /// <param name="error">The exception that occurred during flag evaluation.</param>
    /// <param name="hints">Optional dictionary of hints that can modify hook behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A completed <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// In .NET 9.0 or greater, this method uses <see cref="Activity.AddException(System.Exception)"/>.
    /// In earlier .NET versions, it creates a custom exception event with detailed error information.
    /// </remarks>
    public override ValueTask ErrorAsync<T>(HookContext<T> context, System.Exception error,
        IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
#if NET9_0_OR_GREATER
        // For dotnet9 we should use the new API https://learn.microsoft.com/en-gb/dotnet/api/system.diagnostics.activity.addexception?view=net-9.0
        Activity.Current?.AddException(error);
#else
        var tagsCollection = new ActivityTagsCollection
        {
            { TracingConstants.AttributeExceptionType, error.GetType().FullName },
            { TracingConstants.AttributeExceptionStacktrace, error.ToString() },
        };
        if (!string.IsNullOrWhiteSpace(error.Message))
        {
            tagsCollection.Add(TracingConstants.AttributeExceptionMessage, error.Message);
        }

        Activity.Current?.AddEvent(new ActivityEvent(TracingConstants.AttributeExceptionEventName, default, tagsCollection));
#endif
        return default;
    }
}
