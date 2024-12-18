using OpenFeature.Model;

namespace OpenFeature;

internal class NoOpTransactionContextPropagator : ITransactionContextPropagator
{
    public EvaluationContext GetTransactionContext()
    {
        throw new System.NotImplementedException();
    }

    public void SetTransactionContext(EvaluationContext evaluationContext)
    {
        throw new System.NotImplementedException();
    }
}
