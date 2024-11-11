namespace OpenFeature.Model;

/// <summary>
/// Transaction context is a mechanism for adding transaction specific context that is merged with evaluation context
/// prior to flag evaluation. Examples of potential transaction specific context include: a user id, user agent, or
/// request path.
/// </summary>
/// <seealso href="https://github.com/open-feature/spec/blob/v0.8.0/specification/sections/03-evaluation-context.md#33-context-propagation"/>
public sealed class TransactionContext : EvaluationContext
{
    internal TransactionContext(Structure content) : base(content)
    {
    }
}
