//
// Licensed under the MIT License.

using System.Collections;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class IntAscNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<int>(a, b, true, true);
}

internal class IntAscNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<int>(a, b, true, false);
}

internal class IntDescNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<int>(a, b, false, true);
}

internal class IntDescNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<int>(a, b, false, false);
}
