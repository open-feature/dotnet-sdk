using System.Collections.Immutable;

namespace OpenFeatureSDK.Model
{
    /// <summary>
    /// A structure containing the one or more hooks and hook hints
    /// The hook and hook hints are added to the list of hooks called during the evaluation process
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/types.md#evaluation-options">Flag Evaluation Options</seealso>
    public class FlagEvaluationOptions
    {
        /// <summary>
        /// A immutable list of <see cref="Hook"/>
        /// </summary>
        public IImmutableList<Hook> Hooks { get; }

        /// <summary>
        /// A immutable dictionary of hook hints
        /// </summary>
        public IImmutableDictionary<string, object> HookHints { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlagEvaluationOptions"/> class.
        /// </summary>
        /// <param name="hooks">An immutable list of hooks to use during evaluation</param>
        /// <param name="hookHints">Optional - a list of hints that are passed through the hook lifecycle</param>
        public FlagEvaluationOptions(IImmutableList<Hook> hooks, IImmutableDictionary<string, object> hookHints = null)
        {
            this.Hooks = hooks;
            this.HookHints = hookHints ?? ImmutableDictionary<string, object>.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlagEvaluationOptions"/> class.
        /// </summary>
        /// <param name="hook">A hook to use during the evaluation</param>
        /// <param name="hookHints">Optional - a list of hints that are passed through the hook lifecycle</param>
        public FlagEvaluationOptions(Hook hook, ImmutableDictionary<string, object> hookHints = null)
        {
            this.Hooks = ImmutableList.Create(hook);
            this.HookHints = hookHints ?? ImmutableDictionary<string, object>.Empty;
        }
    }
}
