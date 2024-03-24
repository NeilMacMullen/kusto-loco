// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;
using KustoLoco.Core.InternalRepresentation;
using Kusto.Language;
using Kusto.Language.Symbols;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal static class BuiltInWindowFunctions
{
    private static readonly Dictionary<FunctionSymbol, WindowFunctionInfo> functions = new();

    static BuiltInWindowFunctions()
    {
        functions.Add(
            Functions.RowCumSum,
            new WindowFunctionInfo(
                new WindowOverloadInfo(new RowCumSumIntFunctionImpl(), ScalarTypes.Int, ScalarTypes.Int,
                    ScalarTypes.Bool),
                new WindowOverloadInfo(new RowCumSumLongFunctionImpl(), ScalarTypes.Long, ScalarTypes.Long,
                    ScalarTypes.Bool),
                new WindowOverloadInfo(new RowCumSumDoubleFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real,
                    ScalarTypes.Bool),
                new WindowOverloadInfo(new RowCumSumTimeSpanFunctionImpl(), ScalarTypes.TimeSpan, ScalarTypes.TimeSpan,
                    ScalarTypes.Bool)));
    }

    public static WindowOverloadInfo GetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments,
        List<Parameter> parameters)
    {
        if (!TryGetOverload(symbol, arguments, parameters, out var overload))
        {
            throw new NotImplementedException(
                $"Window function {symbol.Name}{SchemaDisplay.GetText(symbol)} is not implemented for argument types ({string.Join(", ", arguments.Select(arg => SchemaDisplay.GetText(arg.ResultType)))}).");
        }

        Debug.Assert(overload != null);
        return overload;
    }

    public static bool TryGetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters,
        out WindowOverloadInfo? overload)
    {
        if (!functions.TryGetValue(symbol, out var functionInfo))
        {
            overload = null;
            return false;
        }

        overload = BuiltInsHelper.PickOverload(functionInfo.Overloads, arguments);
        return overload != null;
    }
}