using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenFeature.Error;
using OpenFeature.Model;

namespace OpenFeature
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class LoggingHook : Hook
    {
        private readonly ILogger _logger;
        private readonly bool _includeContext;

        /// <summary>
        /// 
        /// </summary>
        public LoggingHook(ILogger logger, bool includeContext = false)
        {
            this._logger = logger;
            this._includeContext = includeContext;
        }

        /// <inheritdoc/>
        public override ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            var domain = context.ClientMetadata.Name ?? string.Empty;
            var providerName = context.ProviderMetadata.Name ?? string.Empty;
            var defaultValue = context.DefaultValue?.ToString() ?? string.Empty;

            if (this._includeContext)
            {
                var evaluationContextLog = new ExecutionContentLog(context.EvaluationContext);
                this.HookBeforeStageExecuted(domain, providerName, context.FlagKey, defaultValue, evaluationContextLog);
            }
            else
            {
                this.HookBeforeStageExecuted(domain, providerName, context.FlagKey, defaultValue);
            }

            return base.BeforeAsync(context, hints, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask ErrorAsync<T>(HookContext<T> context, Exception error, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            var domain = context.ClientMetadata.Name ?? string.Empty;
            var providerName = context.ProviderMetadata.Name ?? string.Empty;
            var defaultValue = context.DefaultValue?.ToString() ?? string.Empty;

            if (error.GetType() == typeof(FeatureProviderException))
            {
                var featureError = (FeatureProviderException)error;
                featureError.ErrorType;
            }

            if (this._includeContext)
            {
                var evaluationContextLog = new ExecutionContentLog(context.EvaluationContext);
                this.HookErrorStageExecuted(domain, providerName, context.FlagKey, defaultValue, evaluationContextLog);
            }
            else
            {
                this.HookErrorStageExecuted(domain, providerName, context.FlagKey, defaultValue);
            }

            return base.ErrorAsync(context, error, hints, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            var domain = context.ClientMetadata.Name ?? string.Empty;
            var providerName = context.ProviderMetadata.Name ?? string.Empty;
            var defaultValue = context.DefaultValue?.ToString() ?? string.Empty;
            var reason = details.Reason ?? string.Empty;
            var variant = details.Variant ?? string.Empty;
            var value = details.Value?.ToString() ?? string.Empty;

            if (this._includeContext)
            {
                var evaluationContextLog = new ExecutionContentLog(context.EvaluationContext);
                this.HookAfterStageExecuted(domain, providerName, context.FlagKey, defaultValue, evaluationContextLog);
            }
            else
            {
                this.HookAfterStageExecuted(domain, providerName, context.FlagKey, defaultValue);
            }

            return base.AfterAsync(context, details, hints, cancellationToken);
        }

        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Debug,
            Message = "[Before] Domain {Domain} with Provider {ProviderName} {FlagKey} defaults with {DefaultValue} {EvaluationContextLog}")]
        partial void HookBeforeStageExecuted(string domain, string providerName, string flagKey, string defaultValue, ExecutionContentLog? evaluationContextLog = null);

        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Error,
            Message = "[Error] Domain {Domain} with Provider {ProviderName} {FlagKey} defaults with {DefaultValue} {Variant} {Variant} {EvaluationContextLog}")]
        partial void HookErrorStageExecuted(string domain, string providerName, string flagKey, string defaultValue, ExecutionContentLog? evaluationContextLog = null);

        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Debug,
            Message = "[After] Domain {Domain} with Provider {ProviderName} {FlagKey} defaults with {DefaultValue} {EvaluationContextLog}")]
        partial void HookAfterStageExecuted(string domain, string providerName, string flagKey, string defaultValue, ExecutionContentLog? evaluationContextLog = null);

        internal class ExecutionContentLog
        {
            private readonly IImmutableDictionary<string, Value> _values;
            private string? _cachedToString;

            public ExecutionContentLog(EvaluationContext evaluationContent)
            {
                this._values = evaluationContent.AsDictionary();
            }

            public override string ToString()
            {
                if (this._cachedToString == null)
                {
                    var stringBuilder = new StringBuilder();

                    foreach (var kvp in this._values)
                    {
                        stringBuilder.Append(kvp.Key);
                        stringBuilder.Append(':');
                        stringBuilder.Append(kvp.Value.ToString());
                        stringBuilder.Append(Environment.NewLine);
                    }

                    this._cachedToString = stringBuilder.ToString();
                }

                return this._cachedToString;
            }
        }
    }
}
