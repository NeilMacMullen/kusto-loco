using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class TrimWsFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var text = (string?)arguments[0].Value;
        return new ScalarResult(ScalarTypes.String, Do(text));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (TypedBaseColumn<string?>)arguments[0].Column;

        var data = new string[column.RowCount];
        for (var i = 0; i < column.RowCount; i++)
        {
            data[i] = Do(column[i]);
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }

    private static string Do(string? s) => s?.Trim() ?? string.Empty;
}