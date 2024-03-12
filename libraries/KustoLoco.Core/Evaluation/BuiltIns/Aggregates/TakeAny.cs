// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class TakeAnyFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = arguments[0].Column;
        return new ScalarResult(column.Type, column.RowCount > 0 ? column.GetRawDataValue(0) : null);
    }
}