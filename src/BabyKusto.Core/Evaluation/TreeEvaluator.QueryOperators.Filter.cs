// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using BabyKusto.Core.Extensions;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult? VisitFilterOperator(IRFilterOperatorNode node, EvaluationContext context)
        {
            Debug.Assert(context.Left != null);
            var result = new FilterResultsTable(this, context.Left.Value, context, node.Condition);
            return new TabularResult(result);
        }

        private class FilterResultsTable : DerivedTableSourceBase<NoContext>
        {
            private readonly TreeEvaluator _owner;
            private readonly EvaluationContext _context;
            private readonly IRExpressionNode _condition;

            public FilterResultsTable(TreeEvaluator owner, ITableSource input, EvaluationContext context, IRExpressionNode condition)
                : base(input)
            {
                _owner = owner;
                _context = context;
                _condition = condition;
                Type = input.Type;
            }

            public override TableSymbol Type { get; }

            protected override (NoContext NewContext, ITableChunk? NewChunk, bool ShouldBreak) ProcessChunk(NoContext _, ITableChunk chunk)
            {
                var chunkContext = _context with { Chunk = chunk };
                var evaluated = _condition.Accept(_owner, chunkContext);

                if (evaluated is ScalarResult scalar)
                {
                    // Scalar will evaluate to the same value for any chunk, so wecan process the entire chunk at once
                    if ((bool?)scalar.Value == true)
                    {
                        return (default, chunk.ReParent(this), false);
                    }

                    return (default, null, false);
                }
                else if (evaluated is ColumnarResult columnar)
                {
                    var resultColumns = new ColumnBuilder[chunk.Columns.Length];
                    for (int j = 0; j < chunk.Columns.Length; j++)
                    {
                        resultColumns[j] = chunk.Columns[j].CreateBuilder();
                    }

                    var predicateColumn = (Column<bool?>)columnar.Column;
                    for (int i = 0; i < predicateColumn.RowCount; i++)
                    {
                        if (predicateColumn[i] == true)
                        {
                            for (int j = 0; j < chunk.Columns.Length; j++)
                            {
                                resultColumns[j].Add(chunk.Columns[j].RawData.GetValue(i));
                            }
                        }
                    }

                    return (default, new TableChunk(this, resultColumns.Select(c => c.ToColumn()).ToArray()), false);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
