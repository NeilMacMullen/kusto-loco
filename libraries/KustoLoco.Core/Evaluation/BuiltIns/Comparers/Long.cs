//
// Licensed under the MIT License.

using System.Collections;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class LongAscNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<long>(a, b, true, true);
}

internal class LongAscNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<long>(a, b, true, false);
}

internal class LongDescNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<long>(a, b, false, true);
}

internal class LongDescNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<long>(a, b, false, false);
}
