// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class ArrayLengthFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var value = (JsonNode?)arguments[0].Value;
        var array = value as JsonArray;
        return new ScalarResult(ScalarTypes.Long, array == null ? null : array.Count);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (TypedBaseColumn<JsonNode?>)arguments[0].Column;

        var data = new long?[column.RowCount];
        for (var i = 0; i < column.RowCount; i++)
        {
            var array = column[i] as JsonArray;
            data[i] = array == null ? null : array.Count;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}