// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using BabyKusto.Core.Evaluation.BuiltIns.Impl;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns
{
    internal static class BuiltInFunctions
    {
        private static Dictionary<FunctionSymbol, ScalarFunctionInfo> functions = new();

        static BuiltInFunctions()
        {
            functions.Add(
                Functions.MinOf,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new MinOfIntFunctionImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new MinOfLongFunctionImpl(), ScalarTypes.Long, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new MinOfDoubleFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real)));

            functions.Add(Functions.Now, new ScalarFunctionInfo(new ScalarOverloadInfo(new NowFunctionImpl(), ScalarTypes.DateTime)));

            // TODO: Support N-ary functions properly
            functions.Add(
                Functions.Strcat,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String),
                    new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String, ScalarTypes.String),
                    new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String),
                    new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String),
                    new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String)));
        }

        public static ScalarOverloadInfo GetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters)
        {
            if (!TryGetOverload(symbol, arguments, parameters, out var overload))
            {
                throw new NotImplementedException($"Function {symbol.Display} is not implemented for argument types ({string.Join(", ", arguments.Select(arg => arg.ResultType.Display))}).");
            }

            return overload;
        }
        public static bool TryGetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters, out ScalarOverloadInfo overload)
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
