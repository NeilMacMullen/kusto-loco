﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class StrcatFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);
        var builder = new StringBuilder();
        for (var i = 0; i < arguments.Length; i++)
        {
            builder.Append((string?)arguments[i].Value);
        }

        return new ScalarResult(ScalarTypes.String, builder.ToString());
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
        for (var i = 0; i < columns[0].RowCount; i++)
        {
            for (var j = 0; j < columns.Length; j++)
            {
                builder.Append(columns[j][i]);
            }

            data[i] = builder.ToString();

            builder.Clear();
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}