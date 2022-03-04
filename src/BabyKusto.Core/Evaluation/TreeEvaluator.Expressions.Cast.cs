// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult? VisitCastExpression(IRCastExpressionNode node, EvaluationContext context)
        {
            var impl = node.GetOrSetCache<Func<EvaluationResult[], EvaluationResult>>(
                () =>
                {
                    if (node.ResultKind == EvaluatedExpressionKind.Scalar)
                    {
                        if (node.ResultType == ScalarTypes.Int)
                        {
                            return (EvaluationResult[] arguments) =>
                            {
                                Debug.Assert(arguments.Length == 1);
                                var value = ((ScalarResult)arguments[0]).Value;
                                return new ScalarResult(node.ResultType, value == null ? null : Convert.ToInt32(value));
                            };
                        }
                        else if (node.ResultType == ScalarTypes.Long)
                        {
                            return (EvaluationResult[] arguments) =>
                            {
                                Debug.Assert(arguments.Length == 1);
                                var value = ((ScalarResult)arguments[0]).Value;
                                return new ScalarResult(node.ResultType, value == null ? null : Convert.ToInt64(value));
                            };
                        }
                        else if (node.ResultType == ScalarTypes.Real)
                        {
                            return (EvaluationResult[] arguments) =>
                            {
                                Debug.Assert(arguments.Length == 1);
                                var value = ((ScalarResult)arguments[0]).Value;
                                return new ScalarResult(node.ResultType, value == null ? null : Convert.ToDouble(value));
                            };
                        }
                        else if (node.ResultType == ScalarTypes.String)
                        {
                            return (EvaluationResult[] arguments) =>
                            {
                                Debug.Assert(arguments.Length == 1);
                                var value = ((ScalarResult)arguments[0]).Value;
                                return new ScalarResult(node.ResultType, value == null ? null : Convert.ToString(value));
                            };
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unexpected target cast type for scalar value: {node.ResultType.Display}");
                        }
                    }
                    else if (node.ResultKind == EvaluatedExpressionKind.Columnar)
                    {
                        if (node.ResultType == ScalarTypes.Int)
                        {
                            return (EvaluationResult[] arguments) =>
                            {
                                Debug.Assert(arguments.Length == 1);
                                var columnResult = (ColumnarResult)arguments[0];
                                var builder = new ColumnBuilder<int?>(node.ResultType);
                                columnResult.Column.ForEach(item => builder.Add(item == null ? null : Convert.ToInt32(item)));
                                return new ColumnarResult(builder.ToColumn());
                            };
                        }
                        else if (node.ResultType == ScalarTypes.Long)
                        {
                            return (EvaluationResult[] arguments) =>
                            {
                                Debug.Assert(arguments.Length == 1);
                                var columnResult = (ColumnarResult)arguments[0];
                                var builder = new ColumnBuilder<long?>(node.ResultType);
                                columnResult.Column.ForEach(item => builder.Add(item == null ? null : Convert.ToInt64(item)));
                                return new ColumnarResult(builder.ToColumn());
                            };
                        }
                        else if (node.ResultType == ScalarTypes.Real)
                        {
                            return (EvaluationResult[] arguments) =>
                            {
                                Debug.Assert(arguments.Length == 1);
                                var columnResult = (ColumnarResult)arguments[0];
                                var builder = new ColumnBuilder<double?>(node.ResultType);
                                columnResult.Column.ForEach(item => builder.Add(item == null ? null : Convert.ToDouble(item)));
                                return new ColumnarResult(builder.ToColumn());
                            };
                        }
                        else if (node.ResultType == ScalarTypes.String)
                        {
                            return (EvaluationResult[] arguments) =>
                            {
                                Debug.Assert(arguments.Length == 1);
                                var columnResult = (ColumnarResult)arguments[0];
                                var builder = new ColumnBuilder<string?>(node.ResultType);
                                columnResult.Column.ForEach(item => builder.Add(item == null ? null : Convert.ToString(item)));
                                return new ColumnarResult(builder.ToColumn());
                            };
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unexpected target cast type for columnar value: {node.ResultType.Display}");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unexpected expression result kind {node.ResultKind}");
                    }
                });

            var expressionResult = node.Expression.Accept(this, context);
            Debug.Assert(expressionResult != null);
            return impl(new[] { expressionResult });
        }
    }
}
