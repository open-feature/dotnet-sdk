using System.Collections.Generic;

namespace OpenFeature.Model
{
    public class FlagEvaluationOptions
    {
        public IReadOnlyList<Hook> Hooks { get; }
        public IReadOnlyDictionary<string, object> HookHints { get; }
        
        public FlagEvaluationOptions(IReadOnlyList<Hook> hooks, IReadOnlyDictionary<string, object> hookHints)
        {
            Hooks = hooks;
            HookHints = hookHints;
        }
    }
}