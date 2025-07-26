using System;
using System.Diagnostics;
using System.Text;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class MyMaxOfFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        long? ret = null;
        foreach (var t in arguments)
        {
            if (t.Value is long val)
            {
                if (ret == null)
                    ret = val;
                else
                    ret = Math.Max((long)ret!, val);
            }
        }

        return new ScalarResult(ScalarTypes.Long, ret);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);
        var columns = new TypedBaseColumn<string?>[arguments.Length];
        for (var i = 0; i < arguments.Length; i++)
        {
            columns[i] = (TypedBaseColumn<string?>)arguments[i].Column;
        }

        var data = new string?[columns[0].RowCount];
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            foreach (var t in columns)
            {
                builder.Append(t[row]);
            }

            data[row] = builder.ToString();

            builder.Clear();
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}
