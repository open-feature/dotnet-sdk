using System;
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
            this.FlagKey = flagKey ?? throw new ArgumentNullException(nameof(flagKey));
            this.DefaultValue = defaultValue;
            this.FlagValueType = flagValueType;
            this.ClientMetadata = clientMetadata ?? throw new ArgumentNullException(nameof(clientMetadata));
            this.ProviderMetadata = providerMetadata ?? throw new ArgumentNullException(nameof(providerMetadata));
            this.EvaluationContext = evaluationContext ?? throw new ArgumentNullException(nameof(evaluationContext));
        }
    }
}
