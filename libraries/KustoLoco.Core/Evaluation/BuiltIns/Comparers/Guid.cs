// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class GuidAscNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<Guid>(a, b, true, true);
}

internal class GuidAscNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<Guid>(a, b, true, false);
}

internal class GuidDescNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<Guid>(a, b, false, true);
}

internal class GuidDescNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<Guid>(a, b, false, false);
}
