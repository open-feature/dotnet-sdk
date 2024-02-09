using System;
using System.Collections.Generic;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Model;

namespace OpenFeature.Providers.Memory
{
    /// <summary>
    /// Flag representation for the in-memory provider.
    /// </summary>
    public class Flag
    {

    }

    /// <summary>
    /// Flag representation for the in-memory provider.
    /// </summary>
    public sealed class Flag<T> : Flag
    {
        private Dictionary<string, T> Variants;
        private string DefaultVariant;
        private Func<EvaluationContext, string> ContextEvaluator;

        /// <summary>
        /// Flag representation for the in-memory provider.
        /// </summary>
        /// <param name="variants">dictionary of variants and their corresponding values</param>
        /// <param name="defaultVariant">default variant (should match 1 key in variants dictionary)</param>
        /// <param name="contextEvaluator">optional context-sensitive evaluation function</param>
        public Flag(Dictionary<string, T> variants, string defaultVariant, Func<EvaluationContext, string> contextEvaluator = null)
        {
            this.Variants = variants;
            this.DefaultVariant = defaultVariant;
            this.ContextEvaluator = contextEvaluator;
        }

        internal ResolutionDetails<T> Evaluate(string flagKey, T defaultValue, EvaluationContext evaluationContext)
        {
            T value;
            if (this.ContextEvaluator == null)
            {
                if (this.Variants.TryGetValue(this.DefaultVariant, out value))
                {
                    return new ResolutionDetails<T>(
                        flagKey,
                        value,
                        variant: this.DefaultVariant,
                        reason: Reason.Static
                    );
                }
                else
                {
                    throw new GeneralException($"variant {this.DefaultVariant} not found");
                }
            }
            else
            {
                var variant = this.ContextEvaluator.Invoke(evaluationContext);
                this.Variants.TryGetValue(variant, out value);
                if (value == null)
                {
                    throw new GeneralException($"variant {variant} not found");
                }
                else
                {
                    return new ResolutionDetails<T>(
                        flagKey,
                        value,
                        variant: variant,
                        reason: Reason.TargetingMatch
                    );
                }
            }
        }
    }
}
