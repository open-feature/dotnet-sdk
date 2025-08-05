using OpenFeature.Model;

namespace OpenFeature.Hosting.Tests;

internal class NoOpHook : Hook
{
    public override ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        return base.BeforeAsync(context, hints, cancellationToken);
    }

    public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        return base.AfterAsync(context, details, hints, cancellationToken);
    }

    public override ValueTask FinallyAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> evaluationDetails, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        return base.FinallyAsync(context, evaluationDetails, hints, cancellationToken);
    }

    public override ValueTask ErrorAsync<T>(HookContext<T> context, Exception error, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        return base.ErrorAsync(context, error, hints, cancellationToken);
    }
}
