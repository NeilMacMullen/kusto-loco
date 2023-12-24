// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BabyKusto.Core.Extensions;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;
using NLog;

namespace BabyKusto.Core.Evaluation.BuiltIns;

internal static class BuiltInsHelper
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    internal static T? PickOverload<T>(IReadOnlyList<T> overloads, IRExpressionNode[] arguments)
        where T : OverloadInfoBase
    {
        foreach (var overload in overloads)
        {
            if (overload.ParameterTypes.Count != arguments.Length)
            {
                continue;
            }

            var compatible = true;
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                var parameterType = overload.ParameterTypes[i];

                var simplifiedArgType = argument.ResultType.Simplify();
                var simplifiedParamType = parameterType.Simplify();

                var thisCompatible =
                    simplifiedArgType == simplifiedParamType ||
                    (simplifiedArgType is ScalarSymbol scalarArg && simplifiedParamType is ScalarSymbol scalarParam &&
                     scalarParam.IsWiderThan(scalarArg)) ||
                    simplifiedParamType == ScalarTypes.String; // TODO: Is it true that anything is coercible to string?

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

    private static EvaluationResult CreateResultForScalarInvocation(IScalarFunctionImpl impl,
        EvaluationResult[] arguments, TypeSymbol expectedResultType)
    {
        var scalarArgs = new ScalarResult[arguments.Length];
        for (var i = 0; i < arguments.Length; i++)
        {
            scalarArgs[i] = (ScalarResult)arguments[i];
        }

        var result = impl.InvokeScalar(scalarArgs);
        Debug.Assert(result.Type.Simplify() == expectedResultType.Simplify(),
            $"Evaluation produced wrong type {SchemaDisplay.GetText(result.Type)}, expected {SchemaDisplay.GetText(expectedResultType)}");
        return result;
    }

    /// <summary>
    ///     Given a mixture of scalar and column results, ensure all are columnnar
    /// </summary>
    /// <remarks>
    ///     TODO - does this really live here any more since it's used by more than
    ///     just built-ins ?
    /// </remarks>
    public static ColumnarResult[] CreateResultArray(EvaluationResult[] arguments)
    {
        //if there are no columnar arguments they must all be scalar
        //which means that the length of the result is 1;
        //TODO - could make this cleaner by pushing RowCount down to EvaluationResult
        //base and using MAX(rowcount)

        var numRows =
            arguments.OfType<ColumnarResult>()
                .FirstOrDefault()?.Column.RowCount ?? 1;
        var columnarArgs = new ColumnarResult[arguments.Length];

        for (var i = 0; i < arguments.Length; i++)
        {
            if (arguments[i] is ColumnarResult column)
            {
                columnarArgs[i] = column;
            }
            else
            {
                //create a "single value" columnar result to hold the
                //scalar
                var scalarValue = (ScalarResult)arguments[i];
                columnarArgs[i] =
                    new ColumnarResult(
                        ColumnHelpers.CreateFromScalar(scalarValue.Value, scalarValue.Type,
                            numRows));
            }
        }

        return columnarArgs;
    }

    private static EvaluationResult CreateResultForColumnarInvocation(IScalarFunctionImpl impl,
        EvaluationResult[] arguments, TypeSymbol expectedResultType)
    {
        var columnarArgs = CreateResultArray(arguments);
        //TODO - NPM here - if all columns are single-value we can avoid the row-wise 
        //evaluation and therefore not pull data into memory for deferred columns

        //TODO NPM here -this should be parallisable for at least some operations
        //i.e. 1->1 mappings
        //other things such as max/min would need a map/reduce type approach
        //but possibly they are handled elsewhere
        var result = impl.InvokeColumnar(columnarArgs);
        Debug.Assert(result.Type.Simplify() == expectedResultType.Simplify(),
            $"Evaluation produced wrong type {SchemaDisplay.GetText(result.Type)}, expected {SchemaDisplay.GetText(expectedResultType)}");
        return result;
    }

    // TODO: Support named parameters
    public static Func<EvaluationResult[], EvaluationResult> GetScalarImplementation(IScalarFunctionImpl impl,
        EvaluatedExpressionKind resultKind,
        TypeSymbol expectedResultType)
    {
        return resultKind switch
        {
            EvaluatedExpressionKind.Scalar => arguments =>
                CreateResultForScalarInvocation(impl, arguments, expectedResultType),
            EvaluatedExpressionKind.Columnar => arguments =>
                CreateResultForColumnarInvocation(impl, arguments, expectedResultType),
            _ => throw new InvalidOperationException($"Unexpected result kind {resultKind}")
        };
    }

    public static Func<EvaluationResult[], EvaluationResult> GetWindowImplementation(IWindowFunctionImpl impl,
        EvaluatedExpressionKind resultKind,
        TypeSymbol expectedResultType)
    {
        if (resultKind != EvaluatedExpressionKind.Columnar)
        {
            throw new InvalidOperationException($"Unexpected result kind {resultKind}");
        }

        ColumnarResult[]? lastWindowArgs = null;
        ColumnarResult? previousResult = null;
        //TODO - this is very similar to GetScalarImplementation except that we are calling InvokeWindow
        //and passing in lastwindowargs - look at unifying this 
        return arguments =>
        {
            var columnarArgs = CreateResultArray(arguments);

            var result = impl.InvokeWindow(columnarArgs, lastWindowArgs, previousResult);
            Debug.Assert(result.Type.Simplify() == expectedResultType.Simplify(),
                $"Evaluation produced wrong type {SchemaDisplay.GetText(result.Type)}, expected {SchemaDisplay.GetText(expectedResultType)}");
            lastWindowArgs = columnarArgs;
            previousResult = result;
            return result;
        };
    }
}