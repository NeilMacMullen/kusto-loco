// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class DateTimeAscNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<DateTime>(a, b, true, true);
}

internal class DateTimeAscNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<DateTime>(a, b, true, false);
}

internal class DateTimeDescNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<DateTime>(a, b, false, true);
}

internal class DateTimeDescNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<DateTime>(a, b, false, false);
}
