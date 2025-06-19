﻿using System.Collections.Generic;
using Kusto.Language.Symbols;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal static class FunctionFinder
{
    internal static bool TryGetOverload(
        Dictionary<FunctionSymbol, ScalarFunctionInfo> functions,
        FunctionSymbol symbol,
        TypeSymbol returnType,IRExpressionNode[] arguments, List<Parameter> parameters,
        out ScalarOverloadInfo? overload)
    {
        if (!functions.TryGetValue(symbol, out var functionInfo))
        {
            overload = null;
            return false;
        }

        overload = BuiltInsHelper.PickOverload(returnType, functionInfo.Overloads, arguments);
        return overload != null;
    }
}
