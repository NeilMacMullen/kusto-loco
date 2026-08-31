//
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitMvApplyOperator(IRMvApplyOperatorNode node, EvaluationContext context)
    {
        var input = context.Left.Value;
        var inputSchema = input.Type;
        var resultSchema = (TableSymbol)node.ResultType;

        // Intermediate schema = the input columns, with each expanded column retyped to its element type (same order as
        // the input). The subquery is bound against this schema.
        var expandNames = new HashSet<string>(node.Columns.Select(c => c.ColumnSymbol.Name));
        var expandByName = node.Columns.ToDictionary(c => c.ColumnSymbol.Name, c => c.ColumnSymbol);
        // Input columns, with any expanded one retyped to its element type, PLUS any expanded column that is NOT an
        // input column at all — an aliased expression (`p = parse_json(Col)`) introduces a new name the subquery
        // must be able to bind.
        var intermediateColumns = inputSchema.Columns
            .Select(c => expandByName.TryGetValue(c.Name, out var ec) ? ec : c)
            .Concat(node.Columns
                .Select(c => c.ColumnSymbol)
                .Where(cs => IndexOf(inputSchema, cs.Name) < 0))
            .ToArray();
        var intermediateSchema = new TableSymbol(intermediateColumns);

        var outBuilders = ColumnHelpers.CreateBuildersForTable(resultSchema);

        // Materialize the input once so both the expanded expressions and the per-row reads below see the same data
        // (chunk columns are single-pass), then evaluate each expanded expression COLUMNAR over that chunk. The
        // expanded item is an expression, not necessarily a source column — `mv-apply p = parse_json(Col) on (...)`
        // aliases a computed value that is absent from the input schema — so it must be evaluated, never looked up.
        {
            var chunk = ChunkHelpers.Reassemble(input.GetData().ToArray());
            var chunkContext = context with { Chunk = chunk };
            var expandedColumns = node.Columns
                .Select(c => ((ColumnarResult)c.Expression.Accept(this, chunkContext)).Column)
                .ToArray();

            for (var row = 0; row < chunk.RowCount; row++)
            {
                // Read this source row once (input columns are single-pass).
                var sourceRow = new object?[inputSchema.Columns.Count];
                for (var i = 0; i < inputSchema.Columns.Count; i++)
                    sourceRow[i] = chunk.Columns[i].GetRawDataValue(row);

                // The value to expand per expanded column, taken from the evaluated expression for this row.
                var expandedValues = new object?[expandedColumns.Length];
                for (var i = 0; i < expandedColumns.Length; i++)
                    expandedValues[i] = expandedColumns[i].GetRawDataValue(row);

                var subtable = BuildExpandedSubtable(sourceRow, expandedValues, inputSchema, intermediateSchema,
                    intermediateColumns, expandNames, node);

                // An expansion that yields nothing contributes nothing: mv-apply is a lateral join, so a source row
                // whose arrays are all empty produces no output rows (unlike mv-expand, which keeps the row with a
                // null). Running the subquery anyway would resurrect it, because an aggregate over an empty table
                // still emits a row.
                if (subtable.GetData().Sum(c => c.RowCount) == 0) continue;

                var subContext = context with { Left = TabularResult.CreateUnvisualized(subtable) };
                var subResult = (TabularResult)node.Subquery.Accept(this, subContext);
                var subSchema = subResult.Value.Type;

                // Each result column comes from the subquery output (by name) if present, otherwise it is a surviving
                // source column, replicated across all of this source row's subquery output rows.
                foreach (var subChunk in subResult.Value.GetData())
                    for (var subRow = 0; subRow < subChunk.RowCount; subRow++)
                        for (var col = 0; col < resultSchema.Columns.Count; col++)
                        {
                            var name = resultSchema.Columns[col].Name;
                            var subIdx = IndexOf(subSchema, name);
                            object? value;
                            if (subIdx >= 0)
                            {
                                value = subChunk.Columns[subIdx].GetRawDataValue(subRow);
                            }
                            else
                            {
                                var srcIdx = IndexOf(inputSchema, name);
                                value = srcIdx >= 0 ? sourceRow[srcIdx] : null;
                            }
                            outBuilders[col].Add(value);
                        }
            }
        }

        var outputColumns = outBuilders.Select(b => b.ToColumn()).ToArray();
        return TabularResult.CreateUnvisualized(new InMemoryTableSource(resultSchema, outputColumns));
    }

    private static InMemoryTableSource BuildExpandedSubtable(object?[] sourceRow, object?[] expandedValues,
        TableSymbol inputSchema, TableSymbol intermediateSchema, ColumnSymbol[] intermediateColumns,
        HashSet<string> expandNames, IRMvApplyOperatorNode node)
    {
        // Expand each expanded column's array for this source row; the subtable has as many rows as the longest array.
        // The value comes from the EVALUATED expression (expandedValues), not from a name lookup against the input —
        // the expanded item may be an alias for a computed expression that is absent from the input schema.
        var elementArrays = new Dictionary<string, object?[]>();
        var maxLen = 0;
        for (var c = 0; c < node.Columns.Count; c++)
        {
            var expandCol = node.Columns[c];
            var arr = JsonArrayHelper.ToObjectArrayOfType(expandedValues[c], expandCol.ColumnSymbol.Type);
            elementArrays[expandCol.ColumnSymbol.Name] = arr;
            if (arr.Length > maxLen) maxLen = arr.Length;
        }
        if (node.RowLimit is { } limit && maxLen > limit) maxLen = (int)limit;

        var builders = ColumnHelpers.CreateBuildersForTable(intermediateSchema);
        for (var i = 0; i < intermediateColumns.Length; i++)
        {
            var colSym = intermediateColumns[i];
            if (expandNames.Contains(colSym.Name))
            {
                var arr = elementArrays[colSym.Name];
                for (var e = 0; e < maxLen; e++) builders[i].Add(e < arr.Length ? arr[e] : null);
            }
            else
            {
                var srcIdx = IndexOf(inputSchema, colSym.Name);
                for (var e = 0; e < maxLen; e++) builders[i].Add(sourceRow[srcIdx]);
            }
        }

        return new InMemoryTableSource(intermediateSchema, builders.Select(b => b.ToColumn()).ToArray());
    }

    private static int IndexOf(TableSymbol schema, string name)
    {
        for (var i = 0; i < schema.Columns.Count; i++)
            if (schema.Columns[i].Name == name) return i;
        return -1;
    }
}
