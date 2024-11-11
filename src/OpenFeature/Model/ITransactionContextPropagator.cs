namespace OpenFeature.Model;

/// <summary>
/// Interface for managing transaction context propagation.
/// </summary>
public interface ITransactionContextPropagator
{
    /// <summary>
    /// Sets the transaction context for the current transaction.
    /// </summary>
    /// <param name="transactionContext">The transaction-specific context to set.</param>
    void SetTransactionContext(TransactionContext transactionContext);

    /// <summary>
    /// Returns the currently defined transaction context using the registered transaction context propagator.
    /// </summary>
    /// <returns>The current transaction context</returns>
    TransactionContext GetTransactionContext();
}
