using System.Collections.Generic;

namespace OpenFeature.Model
{
    public class FlagEvaluationOptions
    {
        public IReadOnlyList<IHook> Hooks { get; }
        public IReadOnlyDictionary<string, object> HookHints { get; }
        
        public FlagEvaluationOptions(IReadOnlyList<IHook> hooks, IReadOnlyDictionary<string, object> hookHints)
        {
            Hooks = hooks;
            HookHints = hookHints;
        }
    }
}