using OpenFeature.Model;

namespace OpenFeature.E2ETests.Utils;

public class BeforeHook : Hook
{
    private readonly EvaluationContext context;

    public BeforeHook(EvaluationContext context)
    {
        this.context = context;
    }

    public override ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
    {
        return new ValueTask<EvaluationContext>(this.context);
    }
}
