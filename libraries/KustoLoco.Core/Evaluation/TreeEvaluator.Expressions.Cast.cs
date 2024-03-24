// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using KustoLoco.Core.InternalRepresentation;
using KustoLoco.Core.Util;
using Kusto.Language.Symbols;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitCastExpression(IRCastExpressionNode node, EvaluationContext context)
    {
        var impl = node.GetOrSetCache<Func<EvaluationResult[], EvaluationResult>>(
            () =>
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
                        Debug.Assert(arguments.Length == 1);
                        var columnResult = (ColumnarResult)arguments[0];
                        var builder = new ColumnBuilder<int?>();
                        columnResult.Column.ForEach(item => builder.Add(item == null ? null : Convert.ToInt32(item)));
                        return new ColumnarResult(builder.ToColumn());
                    },
                    EvaluatedExpressionKind.Columnar when node.ResultType == ScalarTypes.Long => arguments =>
                    {
                        Debug.Assert(arguments.Length == 1);
                        var columnResult = (ColumnarResult)arguments[0];
                        var builder = new ColumnBuilder<long?>();
                        columnResult.Column.ForEach(item => builder.Add(item == null ? null : Convert.ToInt64(item)));
                        return new ColumnarResult(builder.ToColumn());
                    },
                    EvaluatedExpressionKind.Columnar when node.ResultType == ScalarTypes.Real => arguments =>
                    {
                        Debug.Assert(arguments.Length == 1);
                        var columnResult = (ColumnarResult)arguments[0];
                        var builder = new ColumnBuilder<double?>();
                        columnResult.Column.ForEach(item => builder.Add(item == null ? null : Convert.ToDouble(item)));
                        return new ColumnarResult(builder.ToColumn());
                    },
                    EvaluatedExpressionKind.Columnar when node.ResultType == ScalarTypes.String => arguments =>
                    {
                        Debug.Assert(arguments.Length == 1);
                        var columnResult = (ColumnarResult)arguments[0];
                        var builder = new ColumnBuilder<string?>();
                        columnResult.Column.ForEach(item => builder.Add(item == null ? null : Convert.ToString(item)));
                        return new ColumnarResult(builder.ToColumn());
                    },
                    EvaluatedExpressionKind.Columnar when node.ResultType == ScalarTypes.DateTime => arguments =>
                    {
                        Debug.Assert(arguments.Length == 1);
                        var columnResult = (ColumnarResult)arguments[0];
                        var builder = new ColumnBuilder<DateTime?>();
                        columnResult.Column.ForEach(item => builder.Add(item == null ? null : Convert.ToString(item)));
                        return new ColumnarResult(builder.ToColumn());
                    },
                    EvaluatedExpressionKind.Columnar when node.ResultType == ScalarTypes.TimeSpan => arguments =>
                    {
                        Debug.Assert(arguments.Length == 1);
                        var columnResult = (ColumnarResult)arguments[0];
                        var builder = new ColumnBuilder<TimeSpan?>();
                        columnResult.Column.ForEach(item => builder.Add(item == null ? null : Convert.ToString(item)));
                        return new ColumnarResult(builder.ToColumn());
                    },
                    EvaluatedExpressionKind.Columnar => throw new InvalidOperationException(
                        $"Unexpected target cast type for columnar value: {SchemaDisplay.GetText(node.ResultType)}"),
                    _ => throw new InvalidOperationException($"Unexpected expression result kind {node.ResultKind}")
                };
            });

        var expressionResult = node.Expression.Accept(this, context);
        Debug.Assert(expressionResult != null);
        return impl(new[] { expressionResult });
    }
}