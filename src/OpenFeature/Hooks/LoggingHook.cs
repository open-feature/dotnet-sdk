using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenFeature.Model;

namespace OpenFeature.Hooks
{
    /// <summary>
    /// The logging hook is a hook which logs messages during the flag evaluation life-cycle.
    /// </summary>
    public sealed partial class LoggingHook : Hook
    {
        private readonly ILogger _logger;
        private readonly bool _includeContext;

        /// <summary>
        /// Initialise a <see cref="LoggingHook"/> with a <paramref name="logger"/> and optional Evaluation Context. <paramref name="includeContext"/> will
        /// include properties in the <see cref="HookContext{T}.EvaluationContext"/> to the generated logs.
        /// </summary>
        public LoggingHook(ILogger logger, bool includeContext = false)
        {
            this._logger = logger;
            this._includeContext = includeContext;
        }

        /// <inheritdoc/>
        public override ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            var evaluationContext = this._includeContext ? context.EvaluationContext : null;

            var beforeLogContent = new LoggingHookContent(
                context.ClientMetadata.Name,
                context.ProviderMetadata.Name,
                context.FlagKey,
                context.DefaultValue?.ToString(),
                evaluationContext);

            this.HookBeforeStageExecuted(beforeLogContent);

            return base.BeforeAsync(context, hints, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask ErrorAsync<T>(HookContext<T> context, Exception error, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            var evaluationContext = this._includeContext ? context.EvaluationContext : null;

            var beforeLogContent = new LoggingHookContent(
                context.ClientMetadata.Name,
                context.ProviderMetadata.Name,
                context.FlagKey,
                context.DefaultValue?.ToString(),
                evaluationContext);

            this.HookErrorStageExecuted(beforeLogContent);

            return base.ErrorAsync(context, error, hints, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            var evaluationContext = this._includeContext ? context.EvaluationContext : null;

            var beforeLogContent = new LoggingHookContent(
                context.ClientMetadata.Name,
                context.ProviderMetadata.Name,
                context.FlagKey,
                context.DefaultValue?.ToString(),
                evaluationContext);

            this.HookAfterStageExecuted(beforeLogContent);

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

        /// <summary>
        /// Generates a log string with contents provided by the <see cref="LoggingHook"/>.
        /// <para>
        ///     Specification for log contents found at https://github.com/open-feature/spec/blob/d261f68331b94fd8ed10bc72bc0485cfc72a51a8/specification/appendix-a-included-utilities.md#logging-hook
        /// </para>
        /// </summary>
        internal class LoggingHookContent
        {
            private readonly string _domain;
            private readonly string _providerName;
            private readonly string _flagKey;
            private readonly string _defaultValue;
            private readonly EvaluationContext? _evaluationContext;

            public LoggingHookContent(string? domain, string? providerName, string flagKey, string? defaultValue, EvaluationContext? evaluationContext = null)
            {
                this._domain = string.IsNullOrEmpty(domain) ? "missing" : domain!;
                this._providerName = string.IsNullOrEmpty(providerName) ? "missing" : providerName!;
                this._flagKey = flagKey;
                this._defaultValue = string.IsNullOrEmpty(defaultValue) ? "missing" : defaultValue!;
                this._evaluationContext = evaluationContext;
            }

            public override string ToString()
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.Append("Domain:");
                stringBuilder.Append(this._domain);
                stringBuilder.Append(Environment.NewLine);

                stringBuilder.Append("ProviderName:");
                stringBuilder.Append(this._providerName);
                stringBuilder.Append(Environment.NewLine);

                stringBuilder.Append("FlagKey:");
                stringBuilder.Append(this._flagKey);
                stringBuilder.Append(Environment.NewLine);

                stringBuilder.Append("DefaultValue:");
                stringBuilder.Append(this._defaultValue);
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

                return stringBuilder.ToString();
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
