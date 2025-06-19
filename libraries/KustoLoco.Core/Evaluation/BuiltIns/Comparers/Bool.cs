// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal static class BoolComparer
{
    public static int Compare(object? a, object? b, bool asc,bool nullsFirst)
    {
        if (a == b) return 0;

        if (a is not bool boolA)
            return nullsFirst ? -1 : 1;
        
        if (b is not bool boolB)
            return nullsFirst ? 1 : -1;
        
        var ret = boolB.CompareTo(boolA);
        
        return asc ? -ret : ret;
    }
}

internal class BoolAscNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        BoolComparer.Compare(a, b, true, true);
}

internal class BoolAscNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        BoolComparer.Compare(a, b, true, false);
}

internal class BoolDescNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        BoolComparer.Compare(a, b, false, true);
}

internal class BoolDescNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b)
        => BoolComparer.Compare(a, b, false, false);
}
