using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class ToUpperFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var text = (string?)arguments[0].Value;
        return new ScalarResult(ScalarTypes.String, ToUpper(text));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (TypedBaseColumn<string?>)arguments[0].Column;

        var data = new string[column.RowCount];
        for (var i = 0; i < column.RowCount; i++)
        {
            data[i] = ToUpper(column[i]);
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }

    private static string ToUpper(string? s) => s?.ToUpperInvariant() ?? string.Empty;
}