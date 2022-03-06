// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BabyKusto.Core.Evaluation.BuiltIns.Impl;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns
{
    internal static class BuiltInWindowFunctions
    {
        private static Dictionary<FunctionSymbol, WindowFunctionInfo> functions = new();

        static BuiltInWindowFunctions()
        {
            functions.Add(
                Functions.RowCumSum,
                new WindowFunctionInfo(
                    new WindowOverloadInfo(new RowCumSumIntFunctionImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Bool),
                    new WindowOverloadInfo(new RowCumSumLongFunctionImpl(), ScalarTypes.Long, ScalarTypes.Long, ScalarTypes.Bool),
                    new WindowOverloadInfo(new RowCumSumDoubleFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Bool),
                    new WindowOverloadInfo(new RowCumSumTimeSpanFunctionImpl(), ScalarTypes.TimeSpan, ScalarTypes.TimeSpan, ScalarTypes.Bool)));
        }

        public static WindowOverloadInfo GetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters)
        {
            if (!TryGetOverload(symbol, arguments, parameters, out var overload))
            {
                throw new NotImplementedException($"Window function {symbol.Display} is not implemented for argument types ({string.Join(", ", arguments.Select(arg => arg.ResultType.Display))}).");
            }

            Debug.Assert(overload != null);
            return overload;
        }
        public static bool TryGetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters, out WindowOverloadInfo? overload)
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
}
