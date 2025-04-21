using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenFeature.Model;

namespace OpenFeature
{
    /// <summary>
    /// This class manages the execution of hooks.
    /// </summary>
    /// <typeparam name="T">type of the evaluation detail provided to the hooks</typeparam>
    internal partial class HookRunner<T>
    {
        private readonly ImmutableList<Hook> _hooks;

        private readonly List<HookContext<T>> _hookContexts;

        private EvaluationContext _evaluationContext;

        private readonly ILogger _logger;

        /// <summary>
        /// Construct a hook runner instance. Each instance should be used for the execution of a single evaluation.
        /// </summary>
        /// <param name="hooks">
        /// The hooks for the evaluation, these should be in the correct order for the before evaluation stage
        /// </param>
        /// <param name="evaluationContext">
        /// The initial evaluation context, this can be updated as the hooks execute
        /// </param>
        /// <param name="sharedHookContext">
        /// Contents of the initial hook context excluding the evaluation context and hook data
        /// </param>
        /// <param name="logger">Client logger instance</param>
        public HookRunner(ImmutableList<Hook> hooks, EvaluationContext evaluationContext,
            SharedHookContext<T> sharedHookContext,
            ILogger logger)
        {
            this._evaluationContext = evaluationContext;
            this._logger = logger;
            this._hooks = hooks;
            this._hookContexts = new List<HookContext<T>>(hooks.Count);
            for (var i = 0; i < hooks.Count; i++)
            {
                // Create hook instance specific hook context.
                // Hook contexts are instance specific so that the mutable hook data is scoped to each hook.
                this._hookContexts.Add(sharedHookContext.ToHookContext(evaluationContext));
            }
        }

        /// <summary>
        /// Execute before hooks.
        /// </summary>
        /// <param name="hints">Optional hook hints</param>
        /// <param name="cancellationToken">Cancellation token which can cancel hook operations</param>
        /// <returns>Context with any modifications from the before hooks</returns>
        public async Task<EvaluationContext> TriggerBeforeHooksAsync(IImmutableDictionary<string, object>? hints,
            CancellationToken cancellationToken = default)
        {
            var evalContextBuilder = EvaluationContext.Builder();
            evalContextBuilder.Merge(this._evaluationContext);

            for (var i = 0; i < this._hooks.Count; i++)
            {
                var hook = this._hooks[i];
                var hookContext = this._hookContexts[i];

                var resp = await hook.BeforeAsync(hookContext, hints, cancellationToken)
                    .ConfigureAwait(false);
                if (resp != null)
                {
                    evalContextBuilder.Merge(resp);
                    this._evaluationContext = evalContextBuilder.Build();
                    for (var j = 0; j < this._hookContexts.Count; j++)
                    {
                        this._hookContexts[j] = this._hookContexts[j].WithNewEvaluationContext(this._evaluationContext);
                    }
                }
                else
                {
                    this.HookReturnedNull(hook.GetType().Name);
                }
            }

            return this._evaluationContext;
        }

        /// <summary>
        /// Execute the after hooks. These are executed in opposite order of the before hooks.
        /// </summary>
        /// <param name="evaluationDetails">The evaluation details which will be provided to the hook</param>
        /// <param name="hints">Optional hook hints</param>
        /// <param name="cancellationToken">Cancellation token which can cancel hook operations</param>
        public async Task TriggerAfterHooksAsync(FlagEvaluationDetails<T> evaluationDetails,
            IImmutableDictionary<string, object>? hints,
            CancellationToken cancellationToken = default)
        {
            // After hooks run in reverse.
            for (var i = this._hooks.Count - 1; i >= 0; i--)
            {
                var hook = this._hooks[i];
                var hookContext = this._hookContexts[i];
                await hook.AfterAsync(hookContext, evaluationDetails, hints, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Execute the error hooks. These are executed in opposite order of the before hooks.
        /// </summary>
        /// <param name="exception">Exception which triggered the error</param>
        /// <param name="hints">Optional hook hints</param>
        /// <param name="cancellationToken">Cancellation token which can cancel hook operations</param>
        public async Task TriggerErrorHooksAsync(Exception exception,
            IImmutableDictionary<string, object>? hints, CancellationToken cancellationToken = default)
        {
            // Error hooks run in reverse.
            for (var i = this._hooks.Count - 1; i >= 0; i--)
            {
                var hook = this._hooks[i];
                var hookContext = this._hookContexts[i];
                try
                {
                    await hook.ErrorAsync(hookContext, exception, hints, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    this.ErrorHookError(hook.GetType().Name, e);
                }
            }
        }

        /// <summary>
        /// Execute the finally hooks. These are executed in opposite order of the before hooks.
        /// </summary>
        /// <param name="evaluationDetails">The evaluation details which will be provided to the hook</param>
        /// <param name="hints">Optional hook hints</param>
        /// <param name="cancellationToken">Cancellation token which can cancel hook operations</param>
        public async Task TriggerFinallyHooksAsync(FlagEvaluationDetails<T> evaluationDetails,
            IImmutableDictionary<string, object>? hints,
            CancellationToken cancellationToken = default)
        {
            // Finally hooks run in reverse
            for (var i = this._hooks.Count - 1; i >= 0; i--)
            {
                var hook = this._hooks[i];
                var hookContext = this._hookContexts[i];
                try
                {
                    await hook.FinallyAsync(hookContext, evaluationDetails, hints, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    this.FinallyHookError(hook.GetType().Name, e);
                }
            }
        }

        [LoggerMessage(100, LogLevel.Debug, "Hook {HookName} returned null, nothing to merge back into context")]
        partial void HookReturnedNull(string hookName);

        [LoggerMessage(103, LogLevel.Error, "Error while executing Error hook {HookName}")]
        partial void ErrorHookError(string hookName, Exception exception);

        [LoggerMessage(104, LogLevel.Error, "Error while executing Finally hook {HookName}")]
        partial void FinallyHookError(string hookName, Exception exception);
    }
}
