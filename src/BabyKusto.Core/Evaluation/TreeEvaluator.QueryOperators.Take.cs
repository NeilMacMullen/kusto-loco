// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using BabyKusto.Core.Extensions;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult? VisitTakeOperator(IRTakeOperatorNode node, EvaluationContext context)
        {
            Debug.Assert(context.Left != null);

            var countExpressionResult = node.Expression.Accept(this, context);
            Debug.Assert(countExpressionResult != null);
            var count = (ScalarResult)countExpressionResult;
            var result = new TakeResultTable(context.Left.Value, count.Value == null ? 0 : Convert.ToInt32(count.Value));
            return new TabularResult(result);
        }

        private class TakeResultTable : DerivedTableSourceBase<TakeResultTableContext>
        {
            private readonly int _count;

            public TakeResultTable(ITableSource input, int count)
                : base(input)
            {
                _count = count;
            }

            public override TableSymbol Type => Source.Type;

            protected override TakeResultTableContext Init()
            {
                return new TakeResultTableContext
                {
                    Remaining = _count,
                };
            }

            protected override (TakeResultTableContext NewContext, ITableChunk? NewChunk, bool ShouldBreak) ProcessChunk(TakeResultTableContext context, ITableChunk chunk)
            {
                if (context.Remaining >= chunk.RowCount)
                {
                    context.Remaining -= chunk.RowCount;
                    return (context, chunk.ReParent(this), false);
                }
                else
                {
                    var columns = new Column[chunk.Columns.Length];
                    for (int i = 0; i < columns.Length; i++)
                    {
                        columns[i] = chunk.Columns[i].Slice(0, context.Remaining);
                    }

                    var trimmedChunk = new TableChunk(this, columns);
                    context.Remaining = 0;
                    return (context, trimmedChunk, ShouldBreak: true);
                }
            }
        }

        private struct TakeResultTableContext
        {
            public int Remaining;
        }
    }
}
