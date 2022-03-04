// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult? VisitProjectOperator(IRProjectOperatorNode node, EvaluationContext context)
        {
            Debug.Assert(context.Left != null);
            var columns = new List<IROutputColumnNode>(node.Columns.ChildCount);
            for (int i = 0; i < node.Columns.ChildCount; i++)
            {
                columns.Add(node.Columns.GetChild(i));
            }

            var result = new ProjectTableResult(this, context.Left.Value, context, columns, (TableSymbol)node.ResultType);
            return new TabularResult(result);
        }

        private class ProjectTableResult : DerivedTableSourceBase<NoContext>
        {
            private readonly TreeEvaluator _owner;
            private readonly EvaluationContext _context;
            private readonly List<IROutputColumnNode> _columns;

            public ProjectTableResult(TreeEvaluator owner, ITableSource source, EvaluationContext context, List<IROutputColumnNode> columns, TableSymbol resultType)
                : base(source)
            {
                _owner = owner;
                _context = context;
                _columns = columns;
                Type = resultType;
            }

            public override TableSymbol Type { get; }

            protected override (NoContext NewContext, ITableChunk? NewChunk, bool ShouldBreak) ProcessChunk(NoContext _, ITableChunk chunk)
            {
                var outputColumns = new Column[_columns.Count];
                var chunkContext = _context with { Chunk = chunk };
                for (var i = 0; i < _columns.Count; i++)
                {
                    var expression = _columns[i].Expression;
                    var evaluatedExpression = expression.Accept(_owner, chunkContext);
                    Debug.Assert(evaluatedExpression != null);
                    outputColumns[i] = ColumnizeResult(evaluatedExpression, chunk.RowCount);
                }

                return (default, new TableChunk(this, outputColumns), false);
            }

            private static Column ColumnizeResult(EvaluationResult result, int expectedRowCount)
            {
                if (result is ColumnarResult columnarResult)
                {
                    if (columnarResult.Column.RowCount != expectedRowCount)
                    {
                        throw new InvalidOperationException($"Expression produced column with {columnarResult.Column.RowCount} rows but expected {expectedRowCount}.");
                    }

                    return columnarResult.Column;
                }
                else if (result is ScalarResult scalarResult)
                {
                    // Make it into a column of the right size
                    return ColumnHelpers.CreateFromScalar(scalarResult.Value, scalarResult.Type, expectedRowCount);
                }

                throw new InvalidOperationException($"Unexpected expression result is neither a scalar nor a column: {result}.");
            }
        }
    }
}
