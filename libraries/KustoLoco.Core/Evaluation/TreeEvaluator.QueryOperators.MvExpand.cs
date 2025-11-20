//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitMvExpandOperator(IRMvExpandOperatorNode node, EvaluationContext context)
    {
        var result = new MvExpandResultTable(this, context.Left.Value, context, node.Columns, (TableSymbol)node.ResultType);
        return TabularResult.CreateWithVisualisation(result, context.Left.VisualizationState);
    }

    private class MvExpandResultTable : DerivedTableSourceBase<NoContext>
    {
        private readonly EvaluationContext _context;
        private readonly List<IRMvExpandColumnNode> _expandColumns;
        private readonly TreeEvaluator _owner;
        private readonly TableSymbol _resultType;

        public MvExpandResultTable(TreeEvaluator owner, ITableSource input, EvaluationContext context,
            List<IRMvExpandColumnNode> expandColumns, TableSymbol resultType)
            : base(input)
        {
            _owner = owner;
            _context = context;
            _expandColumns = expandColumns;
            _resultType = resultType;
            Type = resultType;
        }

        public override TableSymbol Type { get; }

        protected override (NoContext NewContext, ITableChunk NewChunk, bool ShouldBreak) ProcessChunk(NoContext _,
            ITableChunk chunk)
        {
            var chunkContext = _context with { Chunk = chunk };

            // For simplicity, we'll handle only the case of expanding a single column
            // The general case would need to handle multiple columns being expanded
            if (_expandColumns.Count != 1)
            {
                throw new NotImplementedException("mv-expand with multiple columns is not yet supported");
            }

            var expandColumn = _expandColumns[0];
            
            // Evaluate the column to expand
            var evaluated = expandColumn.Expression.Accept(_owner, chunkContext);
            
            if (evaluated is not ColumnarResult columnar)
            {
                throw new InvalidOperationException("mv-expand requires a columnar result");
            }

            // Get the column that contains arrays to expand
            var sourceColumn = columnar.Column;
            
            // Build the expanded rows
            var expandedRows = new List<ExpandedRow>();
            
            for (var rowIndex = 0; rowIndex < sourceColumn.RowCount; rowIndex++)
            {
                var cellValue = sourceColumn.GetRawDataValue(rowIndex);
                
                // Handle dynamic arrays
                if (cellValue is JsonArray jsonArray)
                {
                    // Create one row for each element in the array
                    foreach (var element in jsonArray)
                    {
                        expandedRows.Add(new ExpandedRow
                        {
                            SourceRowIndex = rowIndex,
                            ExpandedValue = element
                        });
                    }
                }
                else if (cellValue == null)
                {
                    // Null values are preserved as a single row
                    expandedRows.Add(new ExpandedRow
                    {
                        SourceRowIndex = rowIndex,
                        ExpandedValue = null
                    });
                }
                else
                {
                    // Non-array values are treated as single-element arrays
                    expandedRows.Add(new ExpandedRow
                    {
                        SourceRowIndex = rowIndex,
                        ExpandedValue = JsonValue.Create(cellValue)
                    });
                }
            }

            // Build output columns
            // The output schema should match the input schema (same columns, same order)
            var outputColumns = new BaseColumn[chunk.Columns.Length];
            
            for (var colIndex = 0; colIndex < chunk.Columns.Length; colIndex++)
            {
                var sourceCol = chunk.Columns[colIndex];
                var outputColSymbol = _resultType.Columns[colIndex];
                
                // Check if this is the expanded column
                if (outputColSymbol.Name == expandColumn.ColumnSymbol.Name)
                {
                    // Create column with expanded values
                    outputColumns[colIndex] = CreateExpandedColumn(outputColSymbol, expandedRows);
                }
                else
                {
                    // For other columns, duplicate the values from the source rows
                    outputColumns[colIndex] = DuplicateColumn(sourceCol, expandedRows);
                }
            }

            return (default, new TableChunk(this, outputColumns), false);
        }

        private BaseColumn CreateExpandedColumn(ColumnSymbol columnSymbol, List<ExpandedRow> expandedRows)
        {
            var values = expandedRows.Select(r => ConvertJsonNodeToValue(r.ExpandedValue, columnSymbol.Type)).ToArray();
            return ColumnHelpers.CreateFromObjectArray(values, columnSymbol.Type);
        }

        private BaseColumn DuplicateColumn(BaseColumn sourceColumn, List<ExpandedRow> expandedRows)
        {
            var values = expandedRows.Select(r => sourceColumn.GetRawDataValue(r.SourceRowIndex)).ToArray();
            return ColumnHelpers.CreateFromObjectArray(values, sourceColumn.Type);
        }

        private object? ConvertJsonNodeToValue(JsonNode? node, TypeSymbol targetType)
        {
            if (node == null)
                return null;

            // Handle different target types
            if (targetType == ScalarTypes.String)
                return node.ToString();
            
            if (targetType == ScalarTypes.Long)
            {
                if (node is JsonValue jsonValue && jsonValue.TryGetValue<long>(out var longValue))
                    return longValue;
            }
            
            if (targetType == ScalarTypes.Int)
            {
                if (node is JsonValue jsonValue && jsonValue.TryGetValue<int>(out var intValue))
                    return intValue;
            }
            
            if (targetType == ScalarTypes.Real)
            {
                if (node is JsonValue jsonValue && jsonValue.TryGetValue<double>(out var doubleValue))
                    return doubleValue;
            }
            
            if (targetType == ScalarTypes.Bool)
            {
                if (node is JsonValue jsonValue && jsonValue.TryGetValue<bool>(out var boolValue))
                    return boolValue;
            }
            
            if (targetType == ScalarTypes.Dynamic)
            {
                return node;
            }

            // Default: return the node as-is or convert to string
            return node;
        }

        private class ExpandedRow
        {
            public int SourceRowIndex { get; init; }
            public JsonNode? ExpandedValue { get; init; }
        }
    }
}
