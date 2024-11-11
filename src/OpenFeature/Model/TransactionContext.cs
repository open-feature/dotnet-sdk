namespace OpenFeature.Model;

/// <summary>
/// Transaction context is a mechanism for adding transaction specific context that is merged with evaluation context
/// prior to flag evaluation. Examples of potential transaction specific context include: a user id, user agent, or
/// request path.
/// </summary>
public sealed class TransactionContext : EvaluationContext
{
    internal TransactionContext(Structure content) : base(content)
    {
    }
}
