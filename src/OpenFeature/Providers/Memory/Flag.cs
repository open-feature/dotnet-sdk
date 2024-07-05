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
    public interface Flag;

    /// <summary>
    /// Flag representation for the in-memory provider.
    /// </summary>
    public sealed class Flag<T> : Flag
    {
        private readonly Dictionary<string, T> _variants;
        private readonly string _defaultVariant;
        private readonly Func<EvaluationContext, string>? _contextEvaluator;

        /// <summary>
        /// Flag representation for the in-memory provider.
        /// </summary>
        /// <param name="variants">dictionary of variants and their corresponding values</param>
        /// <param name="defaultVariant">default variant (should match 1 key in variants dictionary)</param>
        /// <param name="contextEvaluator">optional context-sensitive evaluation function</param>
        public Flag(Dictionary<string, T> variants, string defaultVariant, Func<EvaluationContext, string>? contextEvaluator = null)
        {
            this._variants = variants;
            this._defaultVariant = defaultVariant;
            this._contextEvaluator = contextEvaluator;
        }

        internal ResolutionDetails<T> Evaluate(string flagKey, T _, EvaluationContext? evaluationContext)
        {
            T? value;
            if (this._contextEvaluator == null)
            {
                if (this._variants.TryGetValue(this._defaultVariant, out value))
                {
                    return new ResolutionDetails<T>(
                        flagKey,
                        value,
                        variant: this._defaultVariant,
                        reason: Reason.Static
                    );
                }
                else
                {
                    throw new GeneralException($"variant {this._defaultVariant} not found");
                }
            }
            else
            {
                var variant = this._contextEvaluator.Invoke(evaluationContext ?? EvaluationContext.Empty);
                if (!this._variants.TryGetValue(variant, out value))
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
