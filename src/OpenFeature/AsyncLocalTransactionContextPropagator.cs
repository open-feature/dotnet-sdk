using System.Threading;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
///  It is a no-op implementation of <see cref="ITransactionContextPropagator"/>
///  It uses the <see cref="AsyncLocal{T}"/> to store the transaction context.
/// </summary>
public sealed class AsyncLocalTransactionContextPropagator : ITransactionContextPropagator
{
    private readonly AsyncLocal<EvaluationContext> _transactionContext = new();

    /// <inheritdoc />
    public EvaluationContext GetTransactionContext()
    {
        return this._transactionContext.Value ?? EvaluationContext.Empty;
    }

    /// <inheritdoc />
    public void SetTransactionContext(EvaluationContext evaluationContext)
    {
        this._transactionContext.Value = evaluationContext;
    }
}
