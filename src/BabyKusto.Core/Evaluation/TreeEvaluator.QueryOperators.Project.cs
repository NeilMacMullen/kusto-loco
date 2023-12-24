// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BabyKusto.Core.Evaluation.BuiltIns;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitProjectOperator(IRProjectOperatorNode node, EvaluationContext context)
    {
        Debug.Assert(context.Left != TabularResult.Empty);
        var columns = new List<IROutputColumnNode>(node.Columns.ChildCount);
        for (var i = 0; i < node.Columns.ChildCount; i++)
        {
            columns.Add(node.Columns.GetTypedChild(i));
        }

        var result = new ProjectTableResult(this, context.Left.Value, context, columns, (TableSymbol)node.ResultType);
        return TabularResult.CreateWithVisualisation(result, context.Left.VisualizationState);
    }

    private class ProjectTableResult : DerivedTableSourceBase<NoContext>
    {
        private readonly List<IROutputColumnNode> _columns;
        private readonly EvaluationContext _context;
        private readonly TreeEvaluator _owner;

        public ProjectTableResult(TreeEvaluator owner, ITableSource source, EvaluationContext context,
            List<IROutputColumnNode> columns, TableSymbol resultType)
            : base(source)
        {
            _owner = owner;
            _context = context;
            _columns = columns;
            Type = resultType;
        }

        public override TableSymbol Type { get; }

        protected override (NoContext NewContext, ITableChunk NewChunk, bool ShouldBreak) ProcessChunk(NoContext _,
            ITableChunk chunk)
        {
            var chunkContext = _context with { Chunk = chunk };
            var results =
                _columns.Select(c => c.Expression.Accept(_owner, chunkContext)).ToArray();
            var outputColumns =
                BuiltInsHelper.CreateResultArray(results)
                    .Select(cr => cr.Column).ToArray();
            return (default, new TableChunk(this, outputColumns), false);
        }
    }
}