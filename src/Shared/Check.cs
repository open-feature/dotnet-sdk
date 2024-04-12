using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OpenFeature;

[DebuggerStepThrough]
static class Check
{
    public static T NotNull<T>(T? value, [CallerArgumentExpression("value")] string name = null!)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(value, name);
#else
        if (value is null)
            throw new ArgumentNullException(name);
#endif

        return value;
    }
}
