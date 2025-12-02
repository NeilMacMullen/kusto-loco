//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitMvExpandOperator(IRMvExpandOperatorNode node, EvaluationContext context)
    {
        var result = new MvExpandResultTable(this, context.Left.Value, context, node.Columns,
            (TableSymbol)node.ResultType);
        return TabularResult.CreateWithVisualisation(result, context.Left.VisualizationState);
    }

    private class MvExpandResultTable : DerivedTableSourceBase<NoContext>
    {
        private readonly EvaluationContext _context;
        private readonly List<IRMvExpandColumnNode> _expandColumns;
        private readonly TreeEvaluator _owner;

        public MvExpandResultTable(TreeEvaluator owner, ITableSource input, EvaluationContext context,
            List<IRMvExpandColumnNode> expandColumns, TableSymbol resultType)
            : base(input)
        {
            _owner = owner;
            _context = context;
            _expandColumns = expandColumns;
            Type = resultType;
        }

        public override TableSymbol Type { get; }

        protected override (NoContext NewContext, ITableChunk NewChunk, bool ShouldBreak) ProcessChunk(NoContext _,
            ITableChunk chunk)
        {
            var chunkContext = _context with { Chunk = chunk };

            // Evaluate all columns to expand and get their source columns and types
            var expandedColumnData = new List<(IRMvExpandColumnNode Column, BaseColumn SourceColumn, TypeSymbol OutputType)>();
            foreach (var expandColumn in _expandColumns)
            {
                var evaluated = expandColumn.Expression.Accept(_owner, chunkContext);
                if (evaluated is not ColumnarResult columnar)
                    throw new InvalidOperationException("mv-expand requires a columnar result");
                
                expandedColumnData.Add((expandColumn, columnar.Column, expandColumn.ColumnSymbol.Type));
            }

            var rowCount = chunk.Columns[0].RowCount;

            // Create builders for each expanded column
            var builders = new List<INullableSetBuilder>();
            foreach (var (_, _, outputType) in expandedColumnData)
            {
                var builder = NullableSetBuilderLocator.GetExpandableNullableSetBuilderForType(
                    TypeMapping.UnderlyingTypeForSymbol(outputType), rowCount);
                builders.Add(builder);
            }

            // Build the expanded rows
            var expandedIndices = new List<int>();

            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                // For each row, get expanded values for all columns
                var expandedValuesPerColumn = new List<object?[]>();
                var maxLength = 0;
                
                foreach (var (_, sourceColumn, outputType) in expandedColumnData)
                {
                    var cellValue = sourceColumn.GetRawDataValue(rowIndex);
                    var expandedObjects = JsonArrayHelper.ToObjectArrayOfType(cellValue, outputType);
                    expandedValuesPerColumn.Add(expandedObjects);
                    if (expandedObjects.Length > maxLength)
                        maxLength = expandedObjects.Length;
                }

                // For each expanded row index (up to maxLength), add values from all columns
                for (var expandIndex = 0; expandIndex < maxLength; expandIndex++)
                {
                    for (var colIdx = 0; colIdx < expandedColumnData.Count; colIdx++)
                    {
                        var expandedValues = expandedValuesPerColumn[colIdx];
                        // Add null if this column's array is shorter than the max
                        var value = expandIndex < expandedValues.Length ? expandedValues[expandIndex] : null;
                        builders[colIdx].Add(value);
                    }
                    expandedIndices.Add(rowIndex);
                }
            }

            var finalIndices = expandedIndices.ToImmutableArray();

            // Build output columns
            // The output schema may have more columns than the input (e.g., when expanding nested paths
            // like properties.ipConfigurations creates a new column properties_ipConfigurations)
            var outputColumns = new BaseColumn[Type.Columns.Count];

            // Build a map of expanded column names to their builders
            var expandedColumnBuilders = new Dictionary<string, INullableSetBuilder>();
            for (var i = 0; i < expandedColumnData.Count; i++)
            {
                expandedColumnBuilders[expandedColumnData[i].Column.ColumnSymbol.Name] = builders[i];
            }

            // Build a map from input chunk column names to column indices
            var inputColumnMap = new Dictionary<string, int>();
            for (var i = 0; i < chunk.Columns.Length; i++)
            {
                // Get the column name from the input table schema
                var inputColName = Source.Type.Columns[i].Name;
                inputColumnMap[inputColName] = i;
            }

            for (var colIndex = 0; colIndex < Type.Columns.Count; colIndex++)
            {
                var colName = Type.Columns[colIndex].Name;
                
                // Check if this is one of the expanded columns
                if (expandedColumnBuilders.TryGetValue(colName, out var builder))
                {
                    // Create column with expanded values
                    outputColumns[colIndex] = ColumnFactory.CreateFromDataSet(builder.ToINullableSet());
                }
                else if (inputColumnMap.TryGetValue(colName, out var inputColIndex))
                {
                    // For columns from the input, duplicate the values from the source rows
                    outputColumns[colIndex] = ColumnHelpers.MapColumn(chunk.Columns[inputColIndex], finalIndices);
                }
                else
                {
                    // This shouldn't happen - the column should either be expanded or from input
                    throw new InvalidOperationException($"Column '{colName}' not found in input or expanded columns");
                }
            }

            return (default, new TableChunk(this, outputColumns), false);
        }
    }
}
