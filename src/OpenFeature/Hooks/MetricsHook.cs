using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Telemetry;

namespace OpenFeature.Hooks;

/// <summary>
/// Represents a hook for capturing metrics related to flag evaluations.
/// The meter instrumentation name is "OpenFeature".
/// </summary>
public class MetricsHook : Hook
{
    private static readonly AssemblyName AssemblyName = typeof(MetricsHook).Assembly.GetName();
    private static readonly string InstrumentationName = AssemblyName.Name ?? "OpenFeature";
    private static readonly string InstrumentationVersion = AssemblyName.Version?.ToString() ?? "1.0.0";
    private static readonly Meter Meter = new(InstrumentationName, InstrumentationVersion);

    private readonly UpDownCounter<long> _evaluationActiveUpDownCounter;
    private readonly Counter<long> _evaluationRequestCounter;
    private readonly Counter<long> _evaluationSuccessCounter;
    private readonly Counter<long> _evaluationErrorCounter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsHook"/> class.
    /// </summary>
    public MetricsHook()
    {
        this._evaluationActiveUpDownCounter = Meter.CreateUpDownCounter<long>(MetricsConstants.ActiveCountName, description: MetricsConstants.ActiveDescription);
        this._evaluationRequestCounter = Meter.CreateCounter<long>(MetricsConstants.RequestsTotalName, "{request}", MetricsConstants.RequestsDescription);
        this._evaluationSuccessCounter = Meter.CreateCounter<long>(MetricsConstants.SuccessTotalName, "{impression}", MetricsConstants.SuccessDescription);
        this._evaluationErrorCounter = Meter.CreateCounter<long>(MetricsConstants.ErrorTotalName, description: MetricsConstants.ErrorDescription);
    }

    /// <inheritdoc/>
    public override ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        var tagList = new TagList
        {
            { TelemetryConstants.Key, context.FlagKey },
            { TelemetryConstants.Provider, context.ProviderMetadata.Name }
        };

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
            { TelemetryConstants.Variant, details.Variant ?? details.Value?.ToString() },
            { TelemetryConstants.Reason, details.Reason ?? Reason.Unknown.ToString() }
        };

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

        this._evaluationActiveUpDownCounter.Add(-1, tagList);

        return base.FinallyAsync(context, evaluationDetails, hints, cancellationToken);
    }
}
