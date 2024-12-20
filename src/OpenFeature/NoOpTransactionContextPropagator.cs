using OpenFeature.Model;

namespace OpenFeature;

internal class NoOpTransactionContextPropagator : ITransactionContextPropagator
{
    public EvaluationContext GetTransactionContext()
    {
        return EvaluationContext.Empty;
    }

    public void SetTransactionContext(EvaluationContext evaluationContext)
    {
    }
}
