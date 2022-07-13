using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenFeature.Model;

namespace OpenFeature
{
    public sealed class OpenFeature
    {
        private EvaluationContext _evaluationContext = new EvaluationContext();
        private IFeatureProvider _featureProvider = new NoOpFeatureProvider();
        private readonly List<Hook> _hooks = new List<Hook>();

        public static OpenFeature Instance { get; } = new OpenFeature();
        static OpenFeature() { }
        private OpenFeature() { }

        public void SetProvider(IFeatureProvider featureProvider) => this._featureProvider = featureProvider;
        public IFeatureProvider GetProvider() => this._featureProvider;
        public Metadata GetProviderMetadata() => this._featureProvider.GetMetadata();
        public FeatureClient GetClient(string name = null, string version = null, ILogger logger = null) => new FeatureClient(this._featureProvider, name, version, logger);

        public void AddHooks(IEnumerable<Hook> hooks) => this._hooks.AddRange(hooks);
        public void AddHooks(Hook hook) => this._hooks.Add(hook);
        public IReadOnlyList<Hook> GetHooks() => this._hooks.AsReadOnly();
        public void ClearHooks() => this._hooks.Clear();

        public void SetContext(EvaluationContext context) => this._evaluationContext = context;
        public EvaluationContext GetContext() => this._evaluationContext;
    }
}
