// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class DecimalAscNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        (a == null && b == null)
            ? 0
            : a == null
                ? -1
                : b == null
                    ? 1
                    : ((decimal)a).CompareTo((decimal)b);
}

internal class DecimalAscNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        (a == null && b == null)
            ? 0
            : a == null
                ? 1
                : b == null
                    ? -1
                    : ((decimal)a).CompareTo((decimal)b);
}

internal class DecimalDescNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        (a == null && b == null)
            ? 0
            : a == null
                ? -1
                : b == null
                    ? 1
                    : ((decimal)b).CompareTo((decimal)a);
}

internal class DecimalDescNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        (a == null && b == null)
            ? 0
            : a == null
                ? 1
                : b == null
                    ? -1
                    : ((decimal)b).CompareTo((decimal)a);
}
