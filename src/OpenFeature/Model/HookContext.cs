using System;
using OpenFeature.Constant;

namespace OpenFeature.Model
{
    /// <summary>
    /// Context provided to hook execution
    /// </summary>
    /// <typeparam name="T">Flag value type</typeparam>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/hooks.md#hook-context"/>
    public class HookContext<T>
    {
        /// <summary>
        /// Feature flag being evaluated
        /// </summary>
        public string FlagKey { get; }

        /// <summary>
        /// Default value if flag fails to be evaluated
        /// </summary>
        public T DefaultValue { get; }

        /// <summary>
        /// The value type of the flag
        /// </summary>
        public FlagValueType FlagValueType { get; }

        /// <summary>
        /// User defined evaluation context used in the evaluation process
        /// <see cref="EvaluationContext"/>
        /// </summary>
        public EvaluationContext EvaluationContext { get; }

        /// <summary>
        /// Client metadata
        /// </summary>
        public ClientMetadata ClientMetadata { get; }

        /// <summary>
        /// Provider metadata
        /// </summary>
        public Metadata ProviderMetadata { get; }

        /// <summary>
        /// Initialize a new instance of <see cref="HookContext{T}"/>
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="flagValueType">Flag value type</param>
        /// <param name="clientMetadata">Client metadata</param>
        /// <param name="providerMetadata">Provider metadata</param>
        /// <param name="evaluationContext">Evaluation context</param>
        /// <exception cref="ArgumentNullException">When any of arguments are null</exception>
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

        internal HookContext<T> WithNewEvaluationContext(EvaluationContext context)
        {
            return new HookContext<T>(
                this.FlagKey,
                this.DefaultValue,
                this.FlagValueType,
                this.ClientMetadata,
                this.ProviderMetadata,
                context
            );
        }
    }
}
