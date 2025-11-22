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

            // For simplicity, we'll handle only the case of expanding a single column
            // The general case would need to handle multiple columns being expanded
            if (_expandColumns.Count != 1)
                throw new NotImplementedException("mv-expand with multiple columns is not yet supported");

            // Evaluate the column to expand
            var expandColumn = _expandColumns[0];

            var evaluated = expandColumn.Expression.Accept(_owner, chunkContext);

            if (evaluated is not ColumnarResult columnar)
                throw new InvalidOperationException("mv-expand requires a columnar result");

            var outputColSymbol = expandColumn.ColumnSymbol.Type;

            // Get the column that contains arrays to expand
            var sourceColumn = columnar.Column;

            //create a builder that can accept objects of the target type (it's probably going to be
            //at least as large as the source so start with that)
            var builder = NullableSetBuilderLocator.GetExpandableNullableSetBuilderForType(
                TypeMapping.UnderlyingTypeForSymbol(outputColSymbol)
                , sourceColumn.RowCount);
           

            // Build the expanded rows
            var expandedIndices = new List<int>();

            for (var rowIndex = 0; rowIndex < sourceColumn.RowCount; rowIndex++)
            {
                var cellValue = sourceColumn.GetRawDataValue(rowIndex);
                var expandedObjects = JsonArrayHelper.ToObjectArrayOfType(cellValue, outputColSymbol);
                foreach (var o in expandedObjects)
                {
                    builder.Add(o);
                    expandedIndices.Add(rowIndex);
                }
            }

            var finalIndices = expandedIndices.ToImmutableArray();

            // Build output columns
            // The output schema should match the input schema (same columns, same order)
            var outputColumns = new BaseColumn[chunk.Columns.Length];

            for (var colIndex = 0; colIndex < chunk.Columns.Length; colIndex++)
                // Check if this is the expanded column
                if (outputColSymbol.Name == expandColumn.ColumnSymbol.Name)
                    // Create column with expanded values
                    outputColumns[colIndex] = ColumnFactory.CreateFromDataSet(builder.ToINullableSet());
                else
                    // For other columns, duplicate the values from the source rows
                    outputColumns[colIndex] = ColumnHelpers.MapColumn(sourceColumn, finalIndices);

            return (default, new TableChunk(this, outputColumns), false);
        }
    }
}
