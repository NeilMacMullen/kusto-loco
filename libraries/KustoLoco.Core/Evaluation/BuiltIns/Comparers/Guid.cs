// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class GuidAscNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        (a == null && b == null)
            ? 0
            : a == null
                ? -1
                : b == null
                    ? 1
                    : ((Guid)a).CompareTo((Guid)b);
}

internal class GuidAscNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        (a == null && b == null)
            ? 0
            : a == null
                ? 1
                : b == null
                    ? -1
                    : ((Guid)a).CompareTo((Guid)b);
}

internal class GuidDescNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        (a == null && b == null)
            ? 0
            : a == null
                ? -1
                : b == null
                    ? 1
                    : ((Guid)b).CompareTo((Guid)a);
}

internal class GuidDescNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        (a == null && b == null)
            ? 0
            : a == null
                ? 1
                : b == null
                    ? -1
                    : ((Guid)b).CompareTo((Guid)a);
}
