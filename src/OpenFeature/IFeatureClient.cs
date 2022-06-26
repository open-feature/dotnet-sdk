using System.Collections.Generic;
using OpenFeature.Model;

namespace OpenFeature
{
    /// <summary>
    /// The FeatureClient interface describes the common client that is used to interact with any feature flag system.
    ///
    /// More information about the feature flag system can be found at:
    /// https://github.com/open-feature/spec/blob/main/specification/evaluation-context.md
    /// https://github.com/open-feature/spec/blob/main/specification/flag-evaluation.md
    /// </summary>
    public interface IFeatureClient
    {
        void AddHooks(IEnumerable<IHook> hooks);
        ClientMetadata GetMetadata();
        
        bool GetBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);
        FlagEvaluationDetails<bool> GetBooleanDetails(string flagKey, bool defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);

        string GetStringValue(string flagKey, string defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);
        FlagEvaluationDetails<string> GetStringDetails(string flagKey, string defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);
        
        int GetNumberValue(string flagKey, int defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);
        FlagEvaluationDetails<int> GetNumberDetails(string flagKey, int defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);
        
        T GetObjectValue<T>(string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);
        FlagEvaluationDetails<T> GetObjectDetails<T>(string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);
    }
}