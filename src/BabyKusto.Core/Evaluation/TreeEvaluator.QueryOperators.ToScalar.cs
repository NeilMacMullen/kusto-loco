// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using BabyKusto.Core.Extensions;
using BabyKusto.Core.InternalRepresentation;

namespace BabyKusto.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitToScalarExpressionNode(IRToScalarExpressionNode node,
        EvaluationContext context)
    {
        var result = node.Expression.Accept(this, context);
        Debug.Assert(result != EvaluationResult.Null);

        if (node.Expression.ResultKind == EvaluatedExpressionKind.Table)
        {
            var table = ((TabularResult)result).Value;

            // TODO: We should have a way to evaluate this asynchronously as well...
            foreach (var chunk in table.GetData())
            {
                if (chunk.Columns[0].RowCount > 0)
                {
                    Debug.Assert(chunk.Columns[0].Type.Simplify() == node.ResultType.Simplify());
                    return new ScalarResult(chunk.Columns[0].Type, chunk.Columns[0].GetRawDataValue(0));
                }
            }

            return new ScalarResult(node.ResultType, null);
        }

        if (node.Expression.ResultKind == EvaluatedExpressionKind.Scalar)
        {
            var scalarValue = (ScalarResult)result;
            return scalarValue;
        }

        throw new InvalidOperationException($"Unexpected result kind {node.Expression.ResultKind}");
    }
}