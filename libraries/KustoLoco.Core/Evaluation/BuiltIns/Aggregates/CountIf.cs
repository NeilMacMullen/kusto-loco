﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class CountIfFunctionImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (GenericTypedBaseColumnOfbool)arguments[0].Column;
        long count = 0;
        for (var i = 0; i < column.RowCount; i++)
        {
            if (column[i] == true)
            {
                count++;
            }
        }

        return new ScalarResult(ScalarTypes.Long, count);
    }
}
