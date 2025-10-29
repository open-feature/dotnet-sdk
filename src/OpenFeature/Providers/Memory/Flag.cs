using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Model;

namespace OpenFeature.Providers.Memory;

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
    private readonly ImmutableMetadata? _flagMetadata;

    /// <summary>
    /// Flag representation for the in-memory provider.
    /// </summary>
    /// <param name="variants">dictionary of variants and their corresponding values</param>
    /// <param name="defaultVariant">default variant (should match 1 key in variants dictionary)</param>
    /// <param name="contextEvaluator">optional context-sensitive evaluation function</param>
    /// <param name="flagMetadata">optional metadata for the flag</param>
    public Flag(Dictionary<string, T> variants, string defaultVariant, Func<EvaluationContext, string>? contextEvaluator = null, ImmutableMetadata? flagMetadata = null)
    {
        this._variants = variants;
        this._defaultVariant = defaultVariant;
        this._contextEvaluator = contextEvaluator;
        this._flagMetadata = flagMetadata;
    }

    internal ResolutionDetails<T> Evaluate(string flagKey, T _, EvaluationContext? evaluationContext)
    {
        if (this._contextEvaluator == null)
        {
            return this.EvaluateDefaultVariant(flagKey);
        }

        string variant;
        try
        {
            variant = this._contextEvaluator.Invoke(evaluationContext ?? EvaluationContext.Empty);
        }
        catch (Exception)
        {
            return this.EvaluateDefaultVariant(flagKey, Reason.Default);
        }

        if (!this._variants.TryGetValue(variant, out var value))
        {
            return this.EvaluateDefaultVariant(flagKey, Reason.Default);
        }

        return new ResolutionDetails<T>(
            flagKey,
            value,
            variant: variant,
            reason: Reason.TargetingMatch,
            flagMetadata: this._flagMetadata
        );
    }

    private ResolutionDetails<T> EvaluateDefaultVariant(string flagKey, string reason = Reason.Static)
    {
        if (this._variants.TryGetValue(this._defaultVariant, out var value))
        {
            return new ResolutionDetails<T>(
                flagKey,
                value,
                variant: this._defaultVariant,
                reason: reason,
                flagMetadata: this._flagMetadata
            );
        }

        throw new GeneralException($"variant {this._defaultVariant} not found");
    }
}
