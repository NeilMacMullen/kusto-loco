//
// Licensed under the MIT License.

using System.Collections;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class DecimalAscNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<decimal>(a, b, true, true);
}

internal class DecimalAscNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<decimal>(a, b, true, false);
}

internal class DecimalDescNullsFirstComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<decimal>(a, b, false, true);
}

internal class DecimalDescNullsLastComparer : IComparer
{
    public int Compare(object? a, object? b) =>
        GenericComparer.CompareT<decimal>(a, b, false, false);
}
