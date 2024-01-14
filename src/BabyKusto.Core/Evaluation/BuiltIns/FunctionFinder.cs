using System.Collections.Generic;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns;

internal static class FunctionFinder
{
    internal static bool TryGetOverload(
        Dictionary<FunctionSymbol, ScalarFunctionInfo> functions,
        FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters,
        out ScalarOverloadInfo? overload)
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