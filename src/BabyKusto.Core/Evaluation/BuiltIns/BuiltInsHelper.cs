// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns
{
    internal static class BuiltInsHelper
    {
        internal static T? PickOverload<T>(IReadOnlyList<T> overloads, IRExpressionNode[] arguments)
            where T : OverloadInfoBase
        {
            foreach (var overload in overloads)
            {
                if (overload.ParameterTypes.Count != arguments.Length)
                {
                    continue;
                }

                bool compatible = true;
                for (int i = 0; i < arguments.Length; i++)
                {
                    var argument = arguments[i];
                    var parameterType = overload.ParameterTypes[i];

                    bool thisCompatible =
                        argument.ResultType == parameterType ||
                        (argument.ResultType is ScalarSymbol scalarArg && parameterType is ScalarSymbol scalarParam && scalarParam.IsWiderThan(scalarArg)) ||
                        parameterType == ScalarTypes.String; // TODO: Is it true that anything is coercible to string?

                    if (!thisCompatible)
                    {
                        compatible = false;
                        break;
                    }
                }

                if (!compatible)
                {
                    continue;
                }

                return overload;
            }

            return null;
        }

        // TODO: Support named parameters
        public static Func<EvaluationResult[], EvaluationResult> GetScalarImplementation(IRExpressionNode[] argumentExpressions, IScalarFunctionImpl impl, EvaluatedExpressionKind resultKind, TypeSymbol expectedResultType)
        {
            if (resultKind == EvaluatedExpressionKind.Scalar)
            {
                int expectedNumArgs = argumentExpressions.Length;
                return (EvaluationResult[] arguments) =>
                {
                    Debug.Assert(arguments.Length == expectedNumArgs);
                    var scalarArgs = new ScalarResult[arguments.Length];
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        scalarArgs[i] = (ScalarResult)arguments[i];
                    }
                    var result = impl.InvokeScalar(scalarArgs);
                    Debug.Assert(result.Type == expectedResultType, $"Evaluation produced wrong type {result.Type.Display}, expected {expectedResultType.Display}");
                    return result;
                };
            }
            else if (resultKind == EvaluatedExpressionKind.Columnar)
            {
                int firstColumnarArgIndex = -1;
                var argNeedsExpansion = new bool[argumentExpressions.Length];
                for (int i = 0; i < argumentExpressions.Length; i++)
                {
                    if (argumentExpressions[i].ResultKind == EvaluatedExpressionKind.Columnar)
                    {
                        if (firstColumnarArgIndex < 0)
                        {
                            firstColumnarArgIndex = i;
                        }
                    }
                    else
                    {
                        argNeedsExpansion[i] = true;
                    }
                }

                Debug.Assert(firstColumnarArgIndex >= 0);
                if (firstColumnarArgIndex < 0)
                {
                    throw new InvalidOperationException();
                }

                return (EvaluationResult[] arguments) =>
                {
                    Debug.Assert(arguments.Length == argNeedsExpansion.Length);

                    var numRows = ((ColumnarResult)arguments[firstColumnarArgIndex]).Column.RowCount;
                    var columnarArgs = new ColumnarResult[arguments.Length];
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (!argNeedsExpansion[i])
                        {
                            columnarArgs[i] = (ColumnarResult)arguments[i];
                        }
                        else
                        {
                            var scalarValue = (ScalarResult)arguments[i];
                            columnarArgs[i] = new ColumnarResult(ColumnHelpers.CreateFromScalar(scalarValue.Value, scalarValue.Type, numRows));
                        }
                    }

                    var result = impl.InvokeColumnar(columnarArgs);
                    Debug.Assert(result.Type == expectedResultType, $"Evaluation produced wrong type {result.Type.Display}, expected {expectedResultType.Display}");
                    return result;
                };
            }
            else
            {
                throw new InvalidOperationException($"Unexpected result kind {resultKind}");
            }
        }

        public static Func<EvaluationResult[], EvaluationResult> GetWindowImplementation(IRExpressionNode[] argumentExpressions, IWindowFunctionImpl impl, EvaluatedExpressionKind resultKind, TypeSymbol expectedResultType)
        {
            if (resultKind != EvaluatedExpressionKind.Columnar)
            {
                throw new InvalidOperationException($"Unexpected result kind {resultKind}");
            }

            int firstColumnarArgIndex = -1;
            var argNeedsExpansion = new bool[argumentExpressions.Length];
            for (int i = 0; i < argumentExpressions.Length; i++)
            {
                if (argumentExpressions[i].ResultKind == EvaluatedExpressionKind.Columnar)
                {
                    if (firstColumnarArgIndex < 0)
                    {
                        firstColumnarArgIndex = i;
                    }
                }
                else
                {
                    argNeedsExpansion[i] = true;
                }
            }

            Debug.Assert(firstColumnarArgIndex >= 0);
            if (firstColumnarArgIndex < 0)
            {
                throw new InvalidOperationException();
            }

            ColumnarResult[]? lastWindowArgs = null;
            ColumnarResult? previousResult = null;
            return (EvaluationResult[] arguments) =>
            {
                Debug.Assert(arguments.Length == argNeedsExpansion.Length);

                var numRows = ((ColumnarResult)arguments[firstColumnarArgIndex]).Column.RowCount;
                var columnarArgs = new ColumnarResult[arguments.Length];
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (!argNeedsExpansion[i])
                    {
                        columnarArgs[i] = (ColumnarResult)arguments[i];
                    }
                    else
                    {
                        var scalarValue = (ScalarResult)arguments[i];
                        columnarArgs[i] = new ColumnarResult(ColumnHelpers.CreateFromScalar(scalarValue.Value, scalarValue.Type, numRows));
                    }
                }

                var result = impl.InvokeWindow(columnarArgs, lastWindowArgs, previousResult);
                Debug.Assert(result.Type == expectedResultType, $"Evaluation produced wrong type {result.Type.Display}, expected {expectedResultType.Display}");
                lastWindowArgs = columnarArgs;
                previousResult = result;
                return result;
            };
        }
    }
}
