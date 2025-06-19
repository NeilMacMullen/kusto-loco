// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class TimeSpanAscNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<TimeSpan>(a, b, true, true);
}

internal class TimeSpanAscNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<TimeSpan>(a, b, true, false);
}

internal class TimeSpanDescNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<TimeSpan>(a, b, false, true);
}

internal class TimeSpanDescNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<TimeSpan>(a, b, true, false);
}
