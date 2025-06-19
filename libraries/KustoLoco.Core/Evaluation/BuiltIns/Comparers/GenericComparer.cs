using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal static class GenericComparer
{
    public static int CompareT<T>(object? a, object? b, bool asc, bool nullsFirst)
        where T:IComparable
    {
        if (a == b) return 0;

        if (a is not T tA)
            return nullsFirst ? -1 : 1;

        if (b is not T tB)
            return nullsFirst ? 1 : -1;

        var ret = tB.CompareTo(tA);

        return asc ? -ret : ret;
    }
}
