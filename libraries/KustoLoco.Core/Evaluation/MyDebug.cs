using System;

namespace KustoLoco.Core.Evaluation;

internal static class MyDebug
{
    public static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    public static void Assert(bool condition)
    {
        if (!condition)
            throw new InvalidOperationException("no message");
    }
}
