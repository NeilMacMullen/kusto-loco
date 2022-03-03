// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using BabyKusto.Core.Extensions;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult VisitFilterOperator(IRFilterOperatorNode node, EvaluationContext context)
        {
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

            protected override (NoContext NewContext, ITableChunk NewChunk, bool ShouldBreak) ProcessChunk(NoContext _, ITableChunk chunk)
            {
                var chunkContext = _context with { Chunk = chunk };
                var evaluated = _condition.Accept(_owner, chunkContext);

                if (evaluated is ScalarResult scalar)
                {
                    if (!Convert.ToBoolean(scalar.Value))
                    {
                        // Scalar will evaluate to the same value for any chunk, so might as well stop now
                        return (default, null, false);
                    }

                    return (default, chunk.ReParent(this), false);
                }
                else if (evaluated is ColumnarResult columnar)
                {
                    var resultColumns = new ColumnBuilder[chunk.Columns.Length];
                    for (int j = 0; j < chunk.Columns.Length; j++)
                    {
                        resultColumns[j] = chunk.Columns[j].CreateBuilder();
                    }

                    for (int i = 0; i < chunk.RowCount; i++)
                    {
                        if (Convert.ToBoolean(columnar.Column.RawData.GetValue(i)))
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
