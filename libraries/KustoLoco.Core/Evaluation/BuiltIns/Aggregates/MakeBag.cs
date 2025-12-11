//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class MakeBagFunctionImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (GenericTypedBaseColumnOfJsonNode)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (GenericTypedBaseColumnOflong)arguments[1].Column;
            MyDebug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var bag = new JsonObject();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            var v = valuesColumn[i];
            if (v == null) continue;

            // make_bag only processes property bags (JsonObject), skips other types
            if (v is JsonObject obj)
            {
                foreach (var kvp in obj)
                {
                    // If key already exists, arbitrary value is selected (we keep the first one)
                    if (!bag.ContainsKey(kvp.Key))
                    {
                        bag[kvp.Key] = kvp.Value?.DeepClone();
                        
                        if (bag.Count >= maxSize)
                        {
                            return new ScalarResult(ScalarTypes.Dynamic, bag);
                        }
                    }
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, bag);
    }
}
