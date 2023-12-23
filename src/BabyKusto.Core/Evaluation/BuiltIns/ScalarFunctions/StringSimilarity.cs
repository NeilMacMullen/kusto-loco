using System.Diagnostics;
using AdvancedStringFunctionality;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class StringSimilarityImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (string?)arguments[0].Value;
        var right = (string?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Real,
                                CalculateSimilarity(left ?? string.Empty, right ?? string.Empty));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var left = (Column<string?>)(arguments[0].Column);
        var right = (Column<string?>)(arguments[1].Column);

        var data = new double?[left.RowCount];
        for (var i = 0; i < left.RowCount; i++)
        {
            data[i] = CalculateSimilarity(left[i] ?? string.Empty, right[i] ?? string.Empty);
        }

        return new ColumnarResult(Column.Create(ScalarTypes.Real, data));
    }

    public double CalculateSimilarity(string left, string right)
    {
        if (left.Length == 0 && right.Length == 0)
            return 100;

        if (left.Length == 0 || right.Length == 0)
        {
            return 0;
        }

        var distance = LevenshteinFunctions.Distance(left, right);
        var smallestString = left.Length < right.Length
                                 ? left
                                 : right;

        var similarity = 100 - (distance * 100 / smallestString.Length);
        return similarity;
    }
}
