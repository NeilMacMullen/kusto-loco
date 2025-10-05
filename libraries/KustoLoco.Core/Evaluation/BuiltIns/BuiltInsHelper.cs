//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kusto.Language.Symbols;
using KustoLoco.Core.Extensions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.Util;
using NLog;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal static class BuiltInsHelper
{
    [Flags]
    enum OverloadOptions
    {
        AllowExactMatch=(1<<1),
        AllowWideningCast=(1<<2),
        AllowStringCast=(1<<3),
    }
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    internal static T? PickOverload<T>(TypeSymbol expectedReturnType, IReadOnlyList<T> overloads,
        IRExpressionNode[] arguments)
        where T : OverloadInfoBase
    {
        //attempt to find the "best" overload by first considering only exact
        //signature matches, then ones that fit larger numbers
        //then finally, those that accept strings
        foreach (var options in new[]
                 {

                     OverloadOptions.AllowExactMatch,
                     OverloadOptions.AllowExactMatch |
                     OverloadOptions.AllowWideningCast,
                     OverloadOptions.AllowExactMatch |
                     OverloadOptions.AllowWideningCast |
                     OverloadOptions.AllowStringCast,

                 })
        {

            if (OverloadInfoBase(expectedReturnType, overloads, arguments,
                    options,
                    out var pickOverload))
                return pickOverload;
        }

        return null;
    }

    private static bool OverloadInfoBase<T>(TypeSymbol expectedReturnType, IReadOnlyList<T> overloads,
        IRExpressionNode[] arguments,
        OverloadOptions options,
        out T? pickOverload) where T : OverloadInfoBase
    {
        pickOverload = null;
        foreach (var overload in overloads)
        {
            var returnType = overload.ReturnType;
            if ((returnType != ScalarTypes.Dynamic) &&
              (returnType.Simplify() != expectedReturnType.Simplify()))
                continue;

            var argumentsToConsider = Math.Min(arguments.Length, overload.NumParametersToMatch);
            var arbitraryCount = overload.RepeatParams;
            if (!arbitraryCount &&  overload.ParameterTypes.Count != argumentsToConsider) continue;

            var compatible = true;
          
            for (var i = 0; i < argumentsToConsider; i++)
            {
                var argument = arguments[i];
                var parameterType = arbitraryCount ?
                    overload.ParameterTypes[0]:
                overload.ParameterTypes[i];

                var simplifiedArgType = argument.ResultType.Simplify();
                var simplifiedParamType = parameterType.Simplify();

                //TODO The parser can sometimes fail to figure out the type of the parameter
                //and therefore emits 'Unknown'  That might need to false matches 
                //-this needs to be reviewed at some point
                var isArgumentSameTypeAsParameter = simplifiedArgType == simplifiedParamType;
                var isParameterWiderThanArgument =
                    simplifiedArgType is ScalarSymbol scalarArg &&
                    simplifiedParamType is ScalarSymbol scalarParam &&
                    scalarParam.IsWiderThan(scalarArg);
                var parameterIsString = simplifiedParamType == ScalarTypes.String;
                var parameterAcceptsAny = simplifiedParamType == ScalarTypes.Unknown;
                var argumentTypeIsUnknown = simplifiedArgType == ScalarTypes.Unknown;
                if (argumentTypeIsUnknown)
                    throw new InvalidOperationException();
                var thisCompatible =
                        (isArgumentSameTypeAsParameter && options.HasFlag(OverloadOptions.AllowExactMatch)) ||
                        (isParameterWiderThanArgument && options.HasFlag(OverloadOptions.AllowWideningCast)) ||
                        (parameterIsString && options.HasFlag(OverloadOptions.AllowStringCast))||
                        argumentTypeIsUnknown
                        || parameterAcceptsAny
                    ;

                if (!thisCompatible)
                {
                    compatible = false;
                    break;
                }
            }

            if (!compatible) continue;


            pickOverload = overload;
            return true;
        }

        return false;
    }

    private static EvaluationResult CreateResultForScalarInvocation(IScalarFunctionImpl impl,
        EvaluationResult[] arguments, TypeSymbol expectedResultType)
    {
        var scalarArgs = arguments.Cast<ScalarResult>().ToArray();
        var result = impl.InvokeScalar(scalarArgs);
        MyDebug.Assert(result.Type.Simplify() == expectedResultType.Simplify(),
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


        var columnarArgs = arguments
            .Select(a => ToColumnar(a, numRows))
            .ToArray();
        return columnarArgs;
    }


    public static ColumnarResult ToColumnar(EvaluationResult r, int numRows)
        => r switch
        {
            ColumnarResult c => c,
            ScalarResult s => new ColumnarResult(
                ColumnHelpers.CreateFromScalar(s.Value, s.Type, numRows)),
            _ => throw new InvalidOperationException()
        };


    private static EvaluationResult CreateResultForColumnarInvocation(IScalarFunctionImpl impl,
        EvaluationResult[] arguments, TypeSymbol expectedResultType, EvaluationHints hints)
    {
        var columnarArgs = CreateResultArray(arguments);


        //TODO NPM here -this should be parallisable for at least some operations
        //i.e. 1->1 mappings
        //other things such as max/min would need a map/reduce type approach
        //but possibly they are handled elsewhere

        //TODO - NPM here - if all columns are single-value we can avoid the row-wise 
        //evaluation and therefore not pull data into memory for deferred columns

        //if columnar args are all SingleValue
        //cast back to Scalars and call InvokeScalar
        //the expand back again

        if (!hints.HasFlag(EvaluationHints.ForceColumnarEvaluation) && columnarArgs.All(c => c.IsSingleValue))
        {
            var logicalRowCount = columnarArgs.Max(c => c.RowCount);
            columnarArgs = columnarArgs.Select(c => c.SliceToTopRow()).ToArray();
            var topRowResult = impl.InvokeColumnar(columnarArgs);
            return topRowResult.Inflate(logicalRowCount);
        }

        var result = impl.InvokeColumnar(columnarArgs);
        MyDebug.Assert(result.Type.Simplify() == expectedResultType.Simplify(),
            $"Evaluation produced wrong type {SchemaDisplay.GetText(result.Type)}, expected {SchemaDisplay.GetText(expectedResultType)}");
        return result;
    }

    // TODO: Support named parameters
    public static Func<EvaluationResult[], EvaluationResult> GetScalarImplementation(IScalarFunctionImpl impl,
        EvaluatedExpressionKind resultKind,
        TypeSymbol expectedResultType, EvaluationHints hints) =>
        resultKind switch
        {
            EvaluatedExpressionKind.Scalar => arguments =>
                CreateResultForScalarInvocation(impl, arguments, expectedResultType),
            EvaluatedExpressionKind.Columnar => arguments =>
                CreateResultForColumnarInvocation(impl, arguments, expectedResultType, hints),
            _ => throw new InvalidOperationException($"Unexpected result kind {resultKind}")
        };

    public static Func<EvaluationResult[], EvaluationResult> GetWindowImplementation(IWindowFunctionImpl impl,
        EvaluatedExpressionKind resultKind,
        TypeSymbol expectedResultType)
    {
        if (resultKind != EvaluatedExpressionKind.Columnar)
            throw new InvalidOperationException($"Unexpected result kind {resultKind}");

        //TODO - this looks completely broken as soon as we try to use chunks or
        //multiple windowed functions !!!
        //we need some proper context to hold the lastwindowarg/previousResult in !!!
        //this is probably why kusto insists that columns should be serialised for
        //windowed functions
        ColumnarResult[]? lastWindowArgs = null;
        ColumnarResult? previousResult = null;
        //TODO - this is very similar to GetScalarImplementation except that we are calling InvokeWindow
        //and passing in lastwindowargs - look at unifying this 
        return arguments =>
        {
            var columnarArgs = CreateResultArray(arguments);

            var result = impl.InvokeWindow(columnarArgs, lastWindowArgs, previousResult);
            MyDebug.Assert(result.Type.Simplify() == expectedResultType.Simplify(),
                $"Evaluation produced wrong type {SchemaDisplay.GetText(result.Type)}, expected {SchemaDisplay.GetText(expectedResultType)}");
            lastWindowArgs = columnarArgs;
            previousResult = result;
            return result;
        };
    }
}
