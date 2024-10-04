using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
            if (this._includeContext)
            {
                var beforeLogContent = new LoggingHookContent(
                    context.ClientMetadata.Name,
                    context.ProviderMetadata.Name,
                    context.FlagKey,
                    context.DefaultValue?.ToString(),
                    context.EvaluationContext);

                this.HookBeforeStageExecuted(beforeLogContent);
            }
            else
            {
                var beforeLogContent = new LoggingHookContent(
                    context.ClientMetadata.Name,
                    context.ProviderMetadata.Name,
                    context.FlagKey,
                    context.DefaultValue?.ToString());

                this.HookBeforeStageExecuted(beforeLogContent);
            }

            return base.BeforeAsync(context, hints, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask ErrorAsync<T>(HookContext<T> context, Exception error, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            if (this._includeContext)
            {
                var beforeLogContent = new LoggingHookContent(
                    context.ClientMetadata.Name,
                    context.ProviderMetadata.Name,
                    context.FlagKey,
                    context.DefaultValue?.ToString(),
                    context.EvaluationContext);

                this.HookErrorStageExecuted(beforeLogContent);
            }
            else
            {
                var beforeLogContent = new LoggingHookContent(
                    context.ClientMetadata.Name,
                    context.ProviderMetadata.Name,
                    context.FlagKey,
                    context.DefaultValue?.ToString());

                this.HookErrorStageExecuted(beforeLogContent);
            }

            return base.ErrorAsync(context, error, hints, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            if (this._includeContext)
            {
                var beforeLogContent = new LoggingHookContent(
                    context.ClientMetadata.Name,
                    context.ProviderMetadata.Name,
                    context.FlagKey,
                    context.DefaultValue?.ToString(),
                    context.EvaluationContext);

                this.HookAfterStageExecuted(beforeLogContent);
            }
            else
            {
                var beforeLogContent = new LoggingHookContent(
                    context.ClientMetadata.Name,
                    context.ProviderMetadata.Name,
                    context.FlagKey,
                    context.DefaultValue?.ToString());

                this.HookAfterStageExecuted(beforeLogContent);
            }

            return base.AfterAsync(context, details, hints, cancellationToken);
        }

        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Debug,
            Message = "Before Flag Evaluation {Content}")]
        partial void HookBeforeStageExecuted(LoggingHookContent content);

        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Error,
            Message = "Error during Flag Evaluation {Content}")]
        partial void HookErrorStageExecuted(LoggingHookContent content);

        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Debug,
            Message = "After Flag Evaluation {Content}")]
        partial void HookAfterStageExecuted(LoggingHookContent content);

        internal class LoggingHookContent
        {
            private readonly string _domain;
            private readonly string _providerName;
            private readonly string _flagKey;
            private readonly string _defaultValue;
            private readonly EvaluationContext? _evaluationContext;

            private string? _cachedToString;

            public LoggingHookContent(string? domain, string? providerName, string flagKey, string? defaultValue, EvaluationContext? evaluationContext = null)
            {
                this._domain = string.IsNullOrEmpty(domain) ? "missing" : domain;
                this._providerName = string.IsNullOrEmpty(providerName) ? "missing" : providerName;
                this._flagKey = flagKey;
                this._defaultValue = string.IsNullOrEmpty(defaultValue) ? "missing" : defaultValue;
                this._evaluationContext = evaluationContext;
            }

            public override string ToString()
            {
                if (this._cachedToString == null)
                {
                    var stringBuilder = new StringBuilder();

                    stringBuilder.Append("Domain:");
                    stringBuilder.Append(_domain);
                    stringBuilder.Append(Environment.NewLine);

                    stringBuilder.Append("ProviderName:");
                    stringBuilder.Append(_providerName);
                    stringBuilder.Append(Environment.NewLine);

                    stringBuilder.Append("FlagKey:");
                    stringBuilder.Append(_flagKey);
                    stringBuilder.Append(Environment.NewLine);

                    stringBuilder.Append("DefaultValue:");
                    stringBuilder.Append(_defaultValue);
                    stringBuilder.Append(Environment.NewLine);

                    if (this._evaluationContext != null)
                    {
                        stringBuilder.Append("Context:");
                        stringBuilder.Append(Environment.NewLine);
                        foreach (var kvp in this._evaluationContext.AsDictionary())
                        {
                            stringBuilder.Append('\t');
                            stringBuilder.Append(kvp.Key);
                            stringBuilder.Append(':');
                            stringBuilder.Append(GetValueString(kvp.Value) ?? "missing");
                            stringBuilder.Append(Environment.NewLine);
                        }
                    }

                    this._cachedToString = stringBuilder.ToString();
                }

                return this._cachedToString;
            }

            static string? GetValueString(Value value)
            {
                if (value.IsNull)
                    return string.Empty;

                if (value.IsString)
                    return value.AsString;

                if (value.IsBoolean)
                    return value.AsBoolean.ToString();

                if (value.IsDateTime)
                    return value.AsDateTime?.ToString("O");

                return value.ToString();
            }
        }
    }
}
