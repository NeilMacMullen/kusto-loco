// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using NotNullStrings;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitCastExpression(IRCastExpressionNode node, EvaluationContext context)
    {
        var impl = node.GetOrSetCache<Func<EvaluationResult[], EvaluationResult>>(() =>
        {
            return node.ResultKind switch
            {
                EvaluatedExpressionKind.Scalar when node.ResultType == ScalarTypes.Int => arguments =>
                {
                    Debug.Assert(arguments.Length == 1);
                    var value = ((ScalarResult)arguments[0]).Value;
                    return new ScalarResult(node.ResultType, value == null ? null : Convert.ToInt32(value));
                },
                EvaluatedExpressionKind.Scalar when node.ResultType == ScalarTypes.Long => arguments =>
                {
                    Debug.Assert(arguments.Length == 1);
                    var value = ((ScalarResult)arguments[0]).Value;
                    return new ScalarResult(node.ResultType, value == null ? null : Convert.ToInt64(value));
                },
                EvaluatedExpressionKind.Scalar when node.ResultType == ScalarTypes.Real => arguments =>
                {
                    Debug.Assert(arguments.Length == 1);
                    var value = ((ScalarResult)arguments[0]).Value;
                    return new ScalarResult(node.ResultType, value == null ? null : Convert.ToDouble(value));
                },
                EvaluatedExpressionKind.Scalar when node.ResultType == ScalarTypes.Decimal => arguments =>
                {
                    Debug.Assert(arguments.Length == 1);
                    var value = ((ScalarResult)arguments[0]).Value;
                    return new ScalarResult(node.ResultType, value == null ? null : Convert.ToDecimal(value));
                }
                ,
                EvaluatedExpressionKind.Scalar when node.ResultType == ScalarTypes.DateTime => arguments =>
                {
                    Debug.Assert(arguments.Length == 1);
                    var value = ((ScalarResult)arguments[0]).Value;
                    return new ScalarResult(node.ResultType, value == null ? null : Convert.ToDateTime(value));
                }
                ,
                EvaluatedExpressionKind.Scalar when node.ResultType == ScalarTypes.String => arguments =>
                {
                    Debug.Assert(arguments.Length == 1);
                    var value = ((ScalarResult)arguments[0]).Value;
                    return new ScalarResult(node.ResultType, value == null ? null : Convert.ToString(value));
                },
                EvaluatedExpressionKind.Scalar => throw new InvalidOperationException(
                    $"Unexpected target cast type for scalar value: {SchemaDisplay.GetText(node.ResultType)}"),
                EvaluatedExpressionKind.Columnar when node.ResultType == ScalarTypes.Int => arguments =>
                {
                    var columnResult = (ColumnarResult)arguments[0];
                    var convertingColumn = GenericConvertingColumnOfint.Create(columnResult.Column,
                        i => i == null ? null : Convert.ToInt32(i));
                    return new ColumnarResult(convertingColumn);
                },
                EvaluatedExpressionKind.Columnar when node.ResultType == ScalarTypes.Long => arguments =>
                {
                    var columnResult = (ColumnarResult)arguments[0];
                    var convertingColumn = GenericConvertingColumnOflong.Create(columnResult.Column,
                        i => i == null ? null : Convert.ToInt64(i));
                    return new ColumnarResult(convertingColumn);
                },
                EvaluatedExpressionKind.Columnar when node.ResultType == ScalarTypes.Decimal => arguments =>
                {
                    var columnResult = (ColumnarResult)arguments[0];
                    var convertingColumn = GenericConvertingColumnOfdecimal.Create(columnResult.Column,
                        i => i == null ? null : Convert.ToDecimal(i));
                    return new ColumnarResult(convertingColumn);
                },
                EvaluatedExpressionKind.Columnar when node.ResultType == ScalarTypes.Real => arguments =>
                {
                    var columnResult = (ColumnarResult)arguments[0];
                    var convertingColumn = GenericConvertingColumnOfdouble.Create(columnResult.Column,
                        i => i == null ? null : Convert.ToDouble(i));
                    return new ColumnarResult(convertingColumn);
                },
                EvaluatedExpressionKind.Columnar when node.ResultType == ScalarTypes.String => arguments =>
                {
                    var columnResult = (ColumnarResult)arguments[0];
                    var convertingColumn = GenericConvertingColumnOfstring.Create(columnResult.Column,
                        i => i == null ? string.Empty : Convert.ToString(i).NullToEmpty());
                    return new ColumnarResult(convertingColumn);
                },
                EvaluatedExpressionKind.Columnar when node.ResultType == ScalarTypes.DateTime => arguments =>
                {
                    var columnResult = (ColumnarResult)arguments[0];
                    var convertingColumn = GenericConvertingColumnOfDateTime.Create(columnResult.Column,
                        i => i == null ? null : Convert.ToDateTime(i));
                    return new ColumnarResult(convertingColumn);
                },
                EvaluatedExpressionKind.Columnar => throw new InvalidOperationException(
                    $"Unexpected target cast type for columnar value: {SchemaDisplay.GetText(node.ResultType)}"),
                _ => throw new InvalidOperationException($"Unexpected expression result kind {node.ResultKind}")
            };
        });

        var expressionResult = node.Expression.Accept(this, context);
        Debug.Assert(expressionResult != null);
        return impl([expressionResult]);
    }
}
