using System.Collections.Generic;

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
        public IReadOnlyList<Hook> Hooks { get; }

        /// <summary>
        /// A immutable dictionary of hook hints
        /// </summary>
        public IReadOnlyDictionary<string, object> HookHints { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlagEvaluationOptions"/> class.
        /// </summary>
        /// <param name="hooks"></param>
        /// <param name="hookHints">Optional - a list of hints that are passed through the hook lifecycle</param>
        public FlagEvaluationOptions(IReadOnlyList<Hook> hooks, IReadOnlyDictionary<string, object> hookHints = null)
        {
            this.Hooks = hooks;
            this.HookHints = hookHints ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlagEvaluationOptions"/> class.
        /// </summary>
        /// <param name="hook"></param>
        /// <param name="hookHints">Optional - a list of hints that are passed through the hook lifecycle</param>
        public FlagEvaluationOptions(Hook hook, IReadOnlyDictionary<string, object> hookHints = null)
        {
            this.Hooks = new[] { hook };
            this.HookHints = hookHints ?? new Dictionary<string, object>();
        }
    }
}
