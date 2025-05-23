using OpenFeature.Constant;

namespace OpenFeature.Model;

/// <summary>
/// Context provided to hook execution
/// </summary>
/// <typeparam name="T">Flag value type</typeparam>
/// <seealso href="https://github.com/open-feature/spec/blob/v0.5.2/specification/sections/04-hooks.md#41-hook-context"/>
public sealed class HookContext<T>
{
    private readonly SharedHookContext<T> _shared;

    /// <summary>
    /// Feature flag being evaluated
    /// </summary>
    public string FlagKey => this._shared.FlagKey;

    /// <summary>
    /// Default value if flag fails to be evaluated
    /// </summary>
    public T DefaultValue => this._shared.DefaultValue;

    /// <summary>
    /// The value type of the flag
    /// </summary>
    public FlagValueType FlagValueType => this._shared.FlagValueType;

    /// <summary>
    /// User defined evaluation context used in the evaluation process
    /// <see cref="EvaluationContext"/>
    /// </summary>
    public EvaluationContext EvaluationContext { get; }

    /// <summary>
    /// Client metadata
    /// </summary>
    public ClientMetadata ClientMetadata => this._shared.ClientMetadata;

    /// <summary>
    /// Provider metadata
    /// </summary>
    public Metadata ProviderMetadata => this._shared.ProviderMetadata;

    /// <summary>
    /// Hook data
    /// </summary>
    public HookData Data { get; }

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
    public HookContext(string? flagKey,
        T defaultValue,
        FlagValueType flagValueType,
        ClientMetadata? clientMetadata,
        Metadata? providerMetadata,
        EvaluationContext? evaluationContext)
    {
        this._shared = new SharedHookContext<T>(
            flagKey, defaultValue, flagValueType, clientMetadata, providerMetadata);

        this.EvaluationContext = evaluationContext ?? throw new ArgumentNullException(nameof(evaluationContext));
        this.Data = new HookData();
    }

    internal HookContext(SharedHookContext<T>? sharedHookContext, EvaluationContext? evaluationContext,
        HookData? hookData)
    {
        this._shared = sharedHookContext ?? throw new ArgumentNullException(nameof(sharedHookContext));
        this.EvaluationContext = evaluationContext ?? throw new ArgumentNullException(nameof(evaluationContext));
        this.Data = hookData ?? throw new ArgumentNullException(nameof(hookData));
    }

    internal HookContext<T> WithNewEvaluationContext(EvaluationContext context)
    {
        return new HookContext<T>(
            this._shared,
            context,
            this.Data
        );
    }
}
