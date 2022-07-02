using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenFeature.Model
{
    /// <summary>
    /// The IHook interface describes the method that a hook can implement to be notified during the lifecycle
    /// of the flag evaluation process.
    ///
    /// More information about the lifecycle of the flag evaluation process can be found here
    /// https://github.com/open-feature/spec/blob/main/specification/hooks.md
    /// </summary>
    public interface IHook
    {
        Task<EvaluationContext> Before<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null);
        Task After<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object> hints = null);
        Task Error<T>(HookContext<T> context, Exception error, IReadOnlyDictionary<string, object> hints = null);
        Task Finally<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null);
    }
}