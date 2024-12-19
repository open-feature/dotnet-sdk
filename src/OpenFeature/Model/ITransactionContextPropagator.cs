namespace OpenFeature.Model;

/// <summary>
/// <see cref="ITransactionContextPropagator"/> is responsible for persisting a transactional context
/// for the duration of a single transaction.
/// Examples of potential transaction specific context include: a user id, user agent, IP.
/// Transaction context is merged with evaluation context prior to flag evaluation.
/// </summary>
/// <remarks>
/// The precedence of merging context can be seen in
/// <a href="https://openfeature.dev/specification/sections/evaluation-context#requirement-323">the specification</a>.
/// </remarks>
public interface ITransactionContextPropagator
{
    /// <summary>
    ///  Returns the currently defined transaction context using the registered transaction context propagator.
    /// </summary>
    /// <returns><see cref="EvaluationContext"/>The current transaction context</returns>
    EvaluationContext GetTransactionContext();

    /// <summary>
    /// Sets the transaction context.
    /// </summary>
    /// <param name="evaluationContext">The transaction context to be set</param>
    void SetTransactionContext(EvaluationContext evaluationContext);
}
