using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using OpenFeature.Model;

namespace OpenFeature.E2ETests.Utils;

[ExcludeFromCodeCoverage]
public class TestHook : Hook
{
    private int _afterCount;
    private int _beforeCount;
    private int _errorCount;
    private int _finallyCount;

    public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details,
        IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        this._afterCount++;
        return base.AfterAsync(context, details, hints, cancellationToken);
    }

    public override ValueTask ErrorAsync<T>(HookContext<T> context, Exception error,
        IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        this._errorCount++;
        return base.ErrorAsync(context, error, hints, cancellationToken);
    }

    public override ValueTask FinallyAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> evaluationDetails,
        IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        this._finallyCount++;
        return base.FinallyAsync(context, evaluationDetails, hints, cancellationToken);
    }

    public override ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context,
        IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        this._beforeCount++;
        return base.BeforeAsync(context, hints, cancellationToken);
    }

    public int AfterCount => this._afterCount;
    public int BeforeCount => this._beforeCount;
    public int ErrorCount => this._errorCount;
    public int FinallyCount => this._finallyCount;
}
