//
// Licensed under the MIT License.

using System.Collections;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class DoubleAscNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<double>(a, b, true, true);
}

internal class DoubleAscNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<double>(a, b, true, false);
}

internal class DoubleDescNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<double>(a, b, false, true);
}

internal class DoubleDescNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<double>(a, b, false, false);
}
