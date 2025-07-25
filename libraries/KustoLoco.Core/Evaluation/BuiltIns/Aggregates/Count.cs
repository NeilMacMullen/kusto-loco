// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class CountFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 0);
        return new ScalarResult(ScalarTypes.Long, (long)chunk.RowCount);
    }
}
