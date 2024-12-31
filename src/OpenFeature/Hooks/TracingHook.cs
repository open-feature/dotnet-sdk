using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OpenFeature.Model;

namespace OpenFeature.Hooks;

/// <summary>
/// Stub.
/// </summary>
public class TracingHook : Hook
{
    /// <inheritdoc/>
    public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details,
        IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        Activity.Current?
            .SetTag("feature_flag.key", details.FlagKey)
            .SetTag("feature_flag.variant", details.Variant)
            .SetTag("feature_flag.provider_name", context.ProviderMetadata.Name)
            .AddEvent(new ActivityEvent("feature_flag", tags: new ActivityTagsCollection
            {
                ["feature_flag.key"] = details.FlagKey,
                ["feature_flag.variant"] = details.Variant,
                ["feature_flag.provider_name"] = context.ProviderMetadata.Name
            }));

        return default;
    }

    /// <inheritdoc/>
    public override ValueTask ErrorAsync<T>(HookContext<T> context, System.Exception error,
        IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
#if NET9_0_OR_GREATER
        // For dotnet9 we should use the new API https://learn.microsoft.com/en-gb/dotnet/api/system.diagnostics.activity.addexception?view=net-9.0
        // Activity.Current?.AddException(error);
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
