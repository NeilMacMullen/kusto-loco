//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitUnionOperator(IRUnionOperatorNode node, EvaluationContext context)
    {
        var tables =
            new List<ITableSource>((context.Left != TabularResult.Empty ? 1 : 0) + node.Expressions.ChildCount);
        var visualizationState = VisualizationState.Empty;
        if (context.Left != TabularResult.Empty)
        {
            tables.Add(context.Left.Value);
            visualizationState = context.Left.VisualizationState;
        }

        for (var i = 0; i < node.Expressions.ChildCount; i++)
        {
            var expression = node.Expressions.GetChild(i);
            var expressionResult = expression.Accept(this, context);
            MyDebug.Assert(expressionResult != EvaluationResult.Null);
            var tableResult = (TabularResult)expressionResult;
            tables.Add(tableResult.Value);

            if (tableResult.VisualizationState != VisualizationState.Empty)
            {
                visualizationState = tableResult.VisualizationState;
            }
        }

        var result = new UnionResultTable(tables, (TableSymbol)node.ResultType);
        return TabularResult.CreateWithVisualisation(result, visualizationState);
    }

    private class UnionResultTable : ITableSource
    {
        //TODO HERE - we should not be keying on ColumnSymbol, but on the original column name/type
        private readonly ColumnSymbolLookup _columnMappings = new();
        private readonly List<ITableSource> _tables;

        public UnionResultTable(List<ITableSource> tables, TableSymbol resultType)
        {
            _tables = tables;
            Type = resultType;


            var i = 0;
            foreach (var columnSymbol in resultType.Members.OfType<ColumnSymbol>())
            {
                _columnMappings.TryAdd(columnSymbol, i);
                foreach (var originalColumn in columnSymbol.OriginalColumns)
                {
                    _columnMappings.TryAdd(originalColumn, i);
                }

                i++;
            }
        }

        public TableSymbol Type { get; }

        public IEnumerable<ITableChunk> GetData()
        {
            foreach (var table in _tables)
            {
                foreach (var chunk in table.GetData())
                {
                    yield return ProcessChunk(table, chunk);
                }
            }
        }

        public async IAsyncEnumerable<ITableChunk> GetDataAsync([EnumeratorCancellation] CancellationToken cancellation)
        {
            foreach (var table in _tables)
            {
                await foreach (var chunk in table.GetDataAsync(cancellation))
                {
                    yield return ProcessChunk(table, chunk);
                }
            }
        }

        private TableChunk ProcessChunk(ITableSource table, ITableChunk chunk)
        {
            var columns = Enumerable.Range(0, Type.Columns.Count).Select(
                    _ => NullColumn.Instance)
                .Cast<BaseColumn>()
                .ToArray();
            for (var i = 0; i < chunk.Columns.Length; i++)
            {
                var symbol = (ColumnSymbol)table.Type.Members[i];
                if (!_columnMappings.TryGetValue(symbol, out var destinationColumnIndex))
                {
                    throw new InvalidOperationException(
                        $"Couldn't find source column to match to output column {SchemaDisplay.GetText(symbol)}");
                }

                columns[destinationColumnIndex] = chunk.Columns[i];
            }

            var rowCount = chunk.RowCount;
            for (var i = 0; i < columns.Length; i++)
            {
                if (columns[i] == NullColumn.Instance)
                {
                    columns[i] = ColumnHelpers.CreateFromScalar(null, Type.Columns[i].Type, rowCount);
                }
            }

            return new TableChunk(this, columns);
        }
    }
}
