// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using BabyKusto.Core.InternalRepresentation;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult? VisitToScalarExpressionNode(IRToScalarExpressionNode node, EvaluationContext context)
        {
            var result = node.Expression.Accept(this, context);
            Debug.Assert(result != null);

            if (node.Expression.ResultKind == EvaluatedExpressionKind.Table)
            {
                var table = ((TabularResult)result).Value;

                // TODO: We should have a way to evaluate this asynchronously as well...
                foreach (var chunk in table.GetData())
                {
                    if (chunk.Columns[0].RowCount > 0)
                    {
                        Debug.Assert(chunk.Columns[0].Type == node.ResultType);
                        return new ScalarResult(chunk.Columns[0].Type, chunk.Columns[0].RawData.GetValue(0));
                    }
                }

                return new ScalarResult(node.ResultType, null);
            }
            else if (node.Expression.ResultKind == EvaluatedExpressionKind.Scalar)
            {
                var scalarValue = (ScalarResult)result;
                return scalarValue;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected result kind {node.Expression.ResultKind}");
            }
        }
    }
}
