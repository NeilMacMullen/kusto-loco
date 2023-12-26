using System.Diagnostics;
using AdvancedStringFunctionality;
using Fastenshtein;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class LevenshteinDistanceImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (string?)arguments[0].Value;
        var right = (string?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Int,
            LevenshteinFunctions.Distance(left, right));
    }


    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var left = (TypedBaseColumn<string?>)(arguments[0].Column);
        var right = (TypedBaseColumn<string?>)(arguments[1].Column);

        var data = new int?[left.RowCount];
        for (var i = 0; i < left.RowCount; i++)
        {
            data[i] = Levenshtein.Distance(left[i], right[i]);
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}