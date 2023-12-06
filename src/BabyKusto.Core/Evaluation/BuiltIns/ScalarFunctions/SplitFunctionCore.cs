﻿using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class SplitFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);

        return new ScalarResult(ScalarTypes.DynamicArrayOfString,
            Evaluate(arguments[0].Value as string, arguments[1].Value as string));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);

        var columns = new Column<string?>[2];
        for (var i = 0; i < arguments.Length; i++)
        {
            columns[i] = (Column<string?>)arguments[i].Column;
        }

        var rowCount = columns[0].RowCount;
        var data = new JsonArray?[rowCount];
        for (var i = 0; i < rowCount; i++)
        {
            data[i] = Evaluate(columns[0][i], columns[1][i]);
        }

        return new ColumnarResult(Column.Create(ScalarTypes.String, data));
    }

    protected JsonArray? Evaluate(string? source, string? delimiter)
    {
        if (source is null) return null;
        if (delimiter is null) return null;

        var toks = source.Split(delimiter);
        var n = toks.Select(t => JsonSerializer.SerializeToNode(t)).ToArray();
        return new JsonArray(n);
    }
}