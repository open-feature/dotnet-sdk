using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenFeature.Model;

namespace OpenFeature
{
    internal interface IHook
    {
        Task<EvaluationContext> Before<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null);
        Task After<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object> hints = null);
        Task Error<T>(HookContext<T> context, Exception error, IReadOnlyDictionary<string, object> hints = null);
        Task Finally<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null);
    }

    /// <summary>
    /// The Hook abstract class describes the default implementation for a hook.
    /// A hook has multiple lifecycles, and is called in the following order when normal execution Before, After, Finally.
    /// When an abnormal execution occurs, the hook is called in the following order: Error, Finally.
    ///
    /// Before: immediately before flag evaluation
    /// After: immediately after successful flag evaluation
    /// Error: immediately after an unsuccessful during flag evaluation
    /// Finally: unconditionally after flag evaluation
    ///
    /// Hooks can be configured to run globally (impacting all flag evaluations), per client, or per flag evaluation invocation.
    ///
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/hooks.md">Hook Specification</seealso>
    public abstract class Hook : IHook
    {
        /// <summary>
        /// Called immediately before flag evaluation.
        /// </summary>
        /// <param name="context">Provides context of innovation</param>
        /// <param name="hints">Caller provided data</param>
        /// <typeparam name="T">Flag value type (bool|number|string|object)</typeparam>
        /// <returns>Modified EvaluationContext that is used for the flag evaluation</returns>
        public virtual Task<EvaluationContext> Before<T>(HookContext<T> context,
            IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.FromResult(new EvaluationContext());
        }

        /// <summary>
        /// Called immediately after successful flag evaluation.
        /// </summary>
        /// <param name="context">Provides context of innovation</param>
        /// <param name="details">Flag evaluation information</param>
        /// <param name="hints">Caller provided data</param>
        /// <typeparam name="T">Flag value type (bool|number|string|object)</typeparam>
        public virtual Task After<T>(HookContext<T> context, FlagEvaluationDetails<T> details,
            IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called immediately after an unsuccessful flag evaluation.
        /// </summary>
        /// <param name="context">Provides context of innovation</param>
        /// <param name="error">Exception representing what went wrong</param>
        /// <param name="hints">Caller provided data</param>
        /// <typeparam name="T">Flag value type (bool|number|string|object)</typeparam>
        public virtual Task Error<T>(HookContext<T> context, Exception error,
            IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called unconditionally after flag evaluation.
        /// </summary>
        /// <param name="context">Provides context of innovation</param>
        /// <param name="hints">Caller provided data</param>
        /// <typeparam name="T">Flag value type (bool|number|string|object)</typeparam>
        public virtual Task Finally<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.CompletedTask;
        }
    }
}
