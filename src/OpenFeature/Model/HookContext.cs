using OpenFeature.Constant;

namespace OpenFeature.Model
{
    public class HookContext<T>
    {
        public string FlagKey { get; }
        public T DefaultValue { get; }
        public FlagValueType FlagValueType { get; }
        public EvaluationContext EvaluationContext { get; }
        public ClientMetadata ClientMetadata { get; }
        public Metadata ProviderMetadata { get; }

        public HookContext(string flagKey,
            T defaultValue,
            FlagValueType flagValueType,
            ClientMetadata clientMetadata,
            Metadata providerMetadata,
            EvaluationContext evaluationContext)
        {
            FlagKey = flagKey;
            DefaultValue = defaultValue;
            FlagValueType = flagValueType;
            ClientMetadata = clientMetadata;
            ProviderMetadata = providerMetadata;
            EvaluationContext = evaluationContext;
        }
    }
}
