// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation
{
    internal partial class IRTranslator
    {
        public override IRNode VisitSummarizeOperator(SummarizeOperator node)
        {
            List<IRExpressionNode> byColumns = new();
            if (node.ByClause != null)
            {
                int byExpressionsCount = node.ByClause.Expressions.Count;
                for (int i = 0; i < byExpressionsCount; i++)
                {
                    var expression = node.ByClause.Expressions[i].Element;
                    var irExpression = (IRExpressionNode)expression.Accept(this);
                    byColumns.Add(irExpression);
                }
            }

            List<IRExpressionNode> aggregations = new();
            for (int i = 0; i < node.Aggregates.Count; i++)
            {
                var expression = node.Aggregates[i].Element;
                var irExpression = (IRExpressionNode)expression.Accept(this);
                aggregations.Add(irExpression);
            }

            return new IRSummarizeOperatorNode(IRListNode.From(aggregations), IRListNode.From(byColumns), node.ResultType);
        }
    }
}
