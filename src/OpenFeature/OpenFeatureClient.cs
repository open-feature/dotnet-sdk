using System;
using System.Collections.Generic;
using OpenFeature.Constant;
using OpenFeature.Extention;
using OpenFeature.Model;

namespace OpenFeature
{
    public class FeatureClient : IFeatureClient
    {
        private readonly ClientMetadata _metadata;
        private readonly List<IHook> _hooks = new List<IHook>();

        public FeatureClient(string name, string version)
        {
            _metadata = new ClientMetadata(name, version);
        }
        
        public ClientMetadata GetMetadata() => _metadata;

        public void AddHooks(IHook hook) => _hooks.Add(hook);
        public void AddHooks(IEnumerable<IHook> hooks) => _hooks.AddRange(hooks); 

        public void ClearHooks() => _hooks.Clear();

        public bool GetBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            GetBooleanDetails(flagKey, defaultValue, context, config).Value;

        public FlagEvaluationDetails<bool> GetBooleanDetails(string flagKey, bool defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            EvaluateFlag(OpenFeature.GetProvider().ResolveBooleanValue, flagKey, defaultValue, context, config);

        public string GetStringValue(string flagKey, string defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            GetStringDetails(flagKey, defaultValue, context, config).Value;
        
        public FlagEvaluationDetails<string> GetStringDetails(string flagKey, string defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            EvaluateFlag(OpenFeature.GetProvider().ResolveStringValue, flagKey, defaultValue, context, config);

        public int GetNumberValue(string flagKey, int defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            GetNumberDetails(flagKey, defaultValue, context, config).Value;
        
        public FlagEvaluationDetails<int> GetNumberDetails(string flagKey, int defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            EvaluateFlag(OpenFeature.GetProvider().ResolveNumberValue, flagKey, defaultValue, context, config);

        public T GetObjectValue<T>(string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null)  =>
            GetObjectDetails(flagKey, defaultValue, context, config).Value;

        public FlagEvaluationDetails<T> GetObjectDetails<T>(string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null)  =>
            EvaluateFlag(OpenFeature.GetProvider().ResolveStructureValue, flagKey, defaultValue, context, config);
        
        private delegate ResolutionDetails<T> ResolveValueDelegate<T>(string flagKey, T defaultValue, EvaluationContext context, FlagEvaluationOptions config);
        
        private static FlagEvaluationDetails<T> EvaluateFlag<T>(ResolveValueDelegate<T> resolveValueDelegate, string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null)
        {
            // New up a evaluation context if one was not provided.
            if (context == null) 
            {
                context = new EvaluationContext();
            }
            
            FlagEvaluationDetails<T> response;
            try
            {
                response = resolveValueDelegate.Invoke(flagKey, defaultValue, context, config)
                    .ToFlagEvaluationDetails();
            }
            catch (Exception e)
            {
                response = new FlagEvaluationDetails<T>(flagKey, defaultValue, e.Message, Reason.Error, string.Empty);
            }
            finally
            {
            }

            return response;
        }
    }
}