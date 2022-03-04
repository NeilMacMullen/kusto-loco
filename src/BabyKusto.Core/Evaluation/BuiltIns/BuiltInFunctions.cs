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
            functions.Add(Functions.Ago, new ScalarFunctionInfo(new ScalarOverloadInfo(new AgoFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.TimeSpan)));

            // TODO: Support N-ary functions properly
            functions.Add(
                Functions.Strcat,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String),
                    new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String, ScalarTypes.String),
                    new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String),
                    new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String),
                    new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String)));

            functions.Add(Functions.Strlen, new ScalarFunctionInfo(new ScalarOverloadInfo(new StrlenFunctionImpl(), ScalarTypes.Long, ScalarTypes.String)));

            var binFunctionInfo = new ScalarFunctionInfo(
                new ScalarOverloadInfo(new BinIntFunctionImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
                new ScalarOverloadInfo(new BinLongFunctionImpl(), ScalarTypes.Long, ScalarTypes.Long, ScalarTypes.Long),
                new ScalarOverloadInfo(new BinDateTimeTimeSpanFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime, ScalarTypes.TimeSpan));
            functions.Add(Functions.Bin, binFunctionInfo);
            functions.Add(Functions.Floor, binFunctionInfo);

            functions.Add(Functions.ToInt, new ScalarFunctionInfo(new ScalarOverloadInfo(new ToIntStringFunctionImpl(), ScalarTypes.Int, ScalarTypes.String)));
            functions.Add(Functions.ToLong, new ScalarFunctionInfo(new ScalarOverloadInfo(new ToLongStringFunctionImpl(), ScalarTypes.Long, ScalarTypes.String)));
            var toDoubleFunctionInfo = new ScalarFunctionInfo(new ScalarOverloadInfo(new ToDoubleStringFunctionImpl(), ScalarTypes.Real, ScalarTypes.String));
            functions.Add(Functions.ToReal, toDoubleFunctionInfo);
            functions.Add(Functions.ToDouble, toDoubleFunctionInfo);
            functions.Add(Functions.ToBool, new ScalarFunctionInfo(new ScalarOverloadInfo(new ToBoolStringFunctionImpl(), ScalarTypes.Bool, ScalarTypes.String)));
        }

        public static ScalarOverloadInfo GetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters)
        {
            if (!TryGetOverload(symbol, arguments, parameters, out var overload))
            {
                throw new NotImplementedException($"Function {symbol.Display} is not implemented for argument types ({string.Join(", ", arguments.Select(arg => arg.ResultType.Display))}).");
            }

            return overload!;
        }
        public static bool TryGetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters, out ScalarOverloadInfo? overload)
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
