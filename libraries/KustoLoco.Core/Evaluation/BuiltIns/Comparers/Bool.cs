// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class BoolAscNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<bool>(a, b, true, true);
}

internal class BoolAscNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<bool>(a, b, true, false);
}

internal class BoolDescNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<bool>(a, b, false, true);
}

internal class BoolDescNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b)
        => GenericComparer.CompareT<bool>(a, b, false, false);
}
