// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Extensions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using NLog;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public override EvaluationResult VisitTakeOperator(IRTakeOperatorNode node, EvaluationContext context)
    {
        Debug.Assert(context.Left != TabularResult.Empty);

        var countExpressionResult = node.Expression.Accept(this, context);
        Debug.Assert(countExpressionResult != EvaluationResult.Null);
        var count = (ScalarResult)countExpressionResult;
        var result =
            new TakeResultTable(context.Left.Value, count.Value == null ? 0 : Convert.ToInt32(count.Value));
        return TabularResult.CreateWithVisualisation(result, context.Left.VisualizationState);
    }

    internal class TakeResultTable : DerivedTableSourceBase<TakeResultTableContext>
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
                Remaining = _count
            };
        }

        protected override (TakeResultTableContext NewContext, ITableChunk NewChunk, bool ShouldBreak)
            ProcessChunk(TakeResultTableContext context, ITableChunk chunk)
        {
            if (context.Remaining >= chunk.RowCount)
            {
                context.Remaining -= chunk.RowCount;
                return (context, chunk.ReParent(this), false);
            }

            var columns = new BaseColumn[chunk.Columns.Length];
            for (var i = 0; i < columns.Length; i++) columns[i] = chunk.Columns[i].Slice(0, context.Remaining);

            var trimmedChunk = new TableChunk(this, columns);
            context.Remaining = 0;
            return (context, trimmedChunk, ShouldBreak: true);
        }
    }

    internal struct TakeResultTableContext
    {
        public int Remaining;
    }
}
