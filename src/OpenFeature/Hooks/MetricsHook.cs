using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Telemetry;

namespace OpenFeature.Hooks;

/// <summary>
/// Represents a hook for capturing metrics related to flag evaluations.
/// The meter instrumentation name is "OpenFeature".
/// </summary>
/// <remarks> This is still experimental and subject to change. </remarks>
public class MetricsHook : Hook
{
    private static readonly AssemblyName AssemblyName = typeof(MetricsHook).Assembly.GetName();
    private static readonly string InstrumentationName = AssemblyName.Name ?? "OpenFeature";
    private static readonly string InstrumentationVersion = AssemblyName.Version?.ToString() ?? "1.0.0";
    private static readonly Meter Meter = new(InstrumentationName, InstrumentationVersion);

    internal readonly UpDownCounter<long> _evaluationActiveUpDownCounter;
    internal readonly Counter<long> _evaluationRequestCounter;
    internal readonly Counter<long> _evaluationSuccessCounter;
    internal readonly Counter<long> _evaluationErrorCounter;

    private readonly MetricsHookOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsHook"/> class.
    /// </summary>
    /// <param name="options">Optional configuration for the metrics hook.</param>
    public MetricsHook(MetricsHookOptions? options = null)
    {
        this._evaluationActiveUpDownCounter = Meter.CreateUpDownCounter<long>(MetricsConstants.ActiveCountName, description: MetricsConstants.ActiveDescription);
        this._evaluationRequestCounter = Meter.CreateCounter<long>(MetricsConstants.RequestsTotalName, "{request}", MetricsConstants.RequestsDescription);
        this._evaluationSuccessCounter = Meter.CreateCounter<long>(MetricsConstants.SuccessTotalName, "{impression}", MetricsConstants.SuccessDescription);
        this._evaluationErrorCounter = Meter.CreateCounter<long>(MetricsConstants.ErrorTotalName, description: MetricsConstants.ErrorDescription);
        this._options = options ?? MetricsHookOptions.Default;
    }

    /// <inheritdoc/>
    public override ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        var tagList = new TagList
        {
            { TelemetryConstants.Key, context.FlagKey },
            { TelemetryConstants.Provider, context.ProviderMetadata.Name }
        };

        this.AddCustomDimensions(ref tagList);
        this.AddFlagMetadataDimensions(null, ref tagList);

        this._evaluationActiveUpDownCounter.Add(1, tagList);
        this._evaluationRequestCounter.Add(1, tagList);

        return base.BeforeAsync(context, hints, cancellationToken);
    }


    /// <inheritdoc/>
    public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        var tagList = new TagList
        {
            { TelemetryConstants.Key, context.FlagKey },
            { TelemetryConstants.Provider, context.ProviderMetadata.Name },
            { TelemetryConstants.Reason, details.Reason ?? Reason.Unknown.ToString() }
        };

        this.AddCustomDimensions(ref tagList);
        this.AddFlagMetadataDimensions(details.FlagMetadata, ref tagList);

        this._evaluationSuccessCounter.Add(1, tagList);

        return base.AfterAsync(context, details, hints, cancellationToken);
    }

    /// <inheritdoc/>
    public override ValueTask ErrorAsync<T>(HookContext<T> context, Exception error, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        var tagList = new TagList
        {
            { TelemetryConstants.Key, context.FlagKey },
            { TelemetryConstants.Provider, context.ProviderMetadata.Name },
            { MetricsConstants.ExceptionAttr, error.Message }
        };

        this.AddCustomDimensions(ref tagList);

        this._evaluationErrorCounter.Add(1, tagList);

        return base.ErrorAsync(context, error, hints, cancellationToken);
    }

    /// <inheritdoc/>
    public override ValueTask FinallyAsync<T>(HookContext<T> context,
        FlagEvaluationDetails<T> evaluationDetails,
        IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        var tagList = new TagList
        {
            { TelemetryConstants.Key, context.FlagKey },
            { TelemetryConstants.Provider, context.ProviderMetadata.Name }
        };

        this.AddCustomDimensions(ref tagList);
        this.AddFlagMetadataDimensions(evaluationDetails.FlagMetadata, ref tagList);

        this._evaluationActiveUpDownCounter.Add(-1, tagList);

        return base.FinallyAsync(context, evaluationDetails, hints, cancellationToken);
    }

    private void AddCustomDimensions(ref TagList tagList)
    {
        foreach (var customDimension in this._options.CustomDimensions)
        {
            tagList.Add(customDimension.Key, customDimension.Value);
        }
    }

    private void AddFlagMetadataDimensions(ImmutableMetadata? flagMetadata, ref TagList tagList)
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
