using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenFeature.Model
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
    ///
    /// More information about the lifecycle of the flag evaluation process can be found here
    /// https://github.com/open-feature/spec/blob/main/specification/hooks.md
    /// </summary>
    public abstract class Hook : IHook
    {
        public virtual Task<EvaluationContext> Before<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.FromResult(new EvaluationContext());
        }

        public virtual Task After<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.CompletedTask;
        }

        public virtual Task Error<T>(HookContext<T> context, Exception error, IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.CompletedTask;
        }

        public virtual Task Finally<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.CompletedTask;
        }
    }
}
