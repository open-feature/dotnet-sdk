using System;
using System.Collections.Generic;
using OpenFeature.Model;

namespace OpenFeature
{
    public sealed class OpenFeature
    {
        private EvaluationContext _evaluationContext;
        private IFeatureProvider _featureProvider = new NoOpFeatureProvider();
        private readonly List<IHook> _hooks = new List<IHook>();
        //TODO Add ILogger interface so we use the standard logging interface
        
        // Thread-safe singleton instance
        private static readonly Lazy<OpenFeature> lazy = new Lazy<OpenFeature>();
        private static OpenFeature Instance => lazy.Value;
        
        public static void SetProvider(IFeatureProvider featureProvider) => Instance._featureProvider = featureProvider;
        public static IFeatureProvider GetProvider() => Instance._featureProvider;
        public static Metadata GetProviderMetadata() => Instance._featureProvider.GetMetadata();
        public static FeatureClient GetClient(string name = null, string version = null) => new FeatureClient(name, version);
        
        public static void AddHooks(IEnumerable<IHook> hooks) => Instance._hooks.AddRange(hooks);
        public static IEnumerable<IHook> GetHooks() => Instance._hooks.AsReadOnly();
        public static void ClearHooks() => Instance._hooks.Clear();

        public static void SetContext(EvaluationContext context) => Instance._evaluationContext = context;
        public static EvaluationContext GetContext() => Instance._evaluationContext;
    }
}