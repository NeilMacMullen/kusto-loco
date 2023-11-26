// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BabyKusto.Core.Extensions;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitFilterOperator(IRFilterOperatorNode node, EvaluationContext context)
    {
        Debug.Assert(context.Left != null);
        var result = new FilterResultsTable(this, context.Left.Value, context, node.Condition);
        return new TabularResult(result, context.Left.VisualizationState);
    }

    private class FilterResultsTable : DerivedTableSourceBase<NoContext>
    {
        private readonly IRExpressionNode _condition;
        private readonly EvaluationContext _context;
        private readonly TreeEvaluator _owner;

        public FilterResultsTable(TreeEvaluator owner, ITableSource input, EvaluationContext context,
            IRExpressionNode condition)
            : base(input)
        {
            _owner = owner;
            _context = context;
            _condition = condition;
            Type = input.Type;
        }

        public override TableSymbol Type { get; }

        protected override (NoContext NewContext, ITableChunk? NewChunk, bool ShouldBreak) ProcessChunk(NoContext _,
            ITableChunk chunk)
        {
            var chunkContext = _context with { Chunk = chunk };
            var evaluated = _condition.Accept(_owner, chunkContext);

            if (evaluated is ScalarResult scalar)
            {
                // Scalar will evaluate to the same value for any chunk, so we can process the entire chunk at once
                if ((bool?)scalar.Value == true)
                {
                    return (default, chunk.ReParent(this), false);
                }

                return (default, null, false);
            }

            if (evaluated is ColumnarResult columnar)
            {
                var predicateColumn = (Column<bool?>)columnar.Column;

                var wantedRows = new List<int>();
                for (var i = 0; i < predicateColumn.RowCount; i++)
                {
                    if (predicateColumn[i] == true)
                    {
                        wantedRows.Add(i);
                    }
                }

                var wanted = wantedRows.ToArray();
                var indirectedColumns = chunk.Columns
                    .Select(c => c.CreateIndirectBuilder(IndirectPolicy.Map))
                    .Select(b => b.CreateIndirectColumn(wanted))
                    .ToArray();
                return (default, new TableChunk(this, indirectedColumns), false);
            }

            throw new InvalidOperationException();
        }
    }
}