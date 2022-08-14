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
    internal static class BuiltInScalarFunctions
    {
        private static Dictionary<FunctionSymbol, ScalarFunctionInfo> functions = new();

        static BuiltInScalarFunctions()
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

            functions.Add(Functions.ReplaceString, new ScalarFunctionInfo(new ScalarOverloadInfo(new ReplaceStringFunctionImpl(), ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String)));

            // TODO: Signature should be `string substring(string, int, int)`. But const literals are evaluated as long's by default and we do not support narrowing at this time.
            functions.Add(
                Functions.Substring,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new SubstringFunctionImpl(), ScalarTypes.String, ScalarTypes.String, ScalarTypes.Long, ScalarTypes.Long)));

            var binFunctionInfo = new ScalarFunctionInfo(
                new ScalarOverloadInfo(new BinIntFunctionImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
                new ScalarOverloadInfo(new BinLongFunctionImpl(), ScalarTypes.Long, ScalarTypes.Long, ScalarTypes.Long),
                new ScalarOverloadInfo(new BinDoubleFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real),
                new ScalarOverloadInfo(new BinDateTimeTimeSpanFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime, ScalarTypes.TimeSpan));
            functions.Add(Functions.Bin, binFunctionInfo);
            functions.Add(Functions.Floor, binFunctionInfo);

            var iffFunctionInfo = new ScalarFunctionInfo(
                new ScalarOverloadInfo(new IffBoolFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Bool, ScalarTypes.Bool, ScalarTypes.Bool),
                new ScalarOverloadInfo(new IffIntFunctionImpl(), ScalarTypes.Int, ScalarTypes.Bool, ScalarTypes.Int, ScalarTypes.Int),
                new ScalarOverloadInfo(new IffLongFunctionImpl(), ScalarTypes.Long, ScalarTypes.Bool, ScalarTypes.Long, ScalarTypes.Long),
                new ScalarOverloadInfo(new IffRealFunctionImpl(), ScalarTypes.Real, ScalarTypes.Bool, ScalarTypes.Real, ScalarTypes.Real),
                new ScalarOverloadInfo(new IffDateTimeFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.Bool, ScalarTypes.DateTime, ScalarTypes.DateTime),
                new ScalarOverloadInfo(new IffTimeSpanFunctionImpl(), ScalarTypes.TimeSpan, ScalarTypes.Bool, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
                new ScalarOverloadInfo(new IffStringFunctionImpl(), ScalarTypes.String, ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String));
            functions.Add(Functions.Iff, iffFunctionInfo);
            functions.Add(Functions.Iif, iffFunctionInfo);

            functions.Add(Functions.ToInt, new ScalarFunctionInfo(new ScalarOverloadInfo(new ToIntStringFunctionImpl(), ScalarTypes.Int, ScalarTypes.String)));
            functions.Add(Functions.ToLong, new ScalarFunctionInfo(new ScalarOverloadInfo(new ToLongStringFunctionImpl(), ScalarTypes.Long, ScalarTypes.String)));
            var toDoubleFunctionInfo = new ScalarFunctionInfo(new ScalarOverloadInfo(new ToDoubleStringFunctionImpl(), ScalarTypes.Real, ScalarTypes.String));
            functions.Add(Functions.ToReal, toDoubleFunctionInfo);
            functions.Add(Functions.ToDouble, toDoubleFunctionInfo);
            functions.Add(Functions.ToBool, new ScalarFunctionInfo(new ScalarOverloadInfo(new ToBoolStringFunctionImpl(), ScalarTypes.Bool, ScalarTypes.String)));

            functions.Add(Functions.UrlEncode_Component, new ScalarFunctionInfo(new ScalarOverloadInfo(new UrlEncodeComponentFunctionImpl(), ScalarTypes.String, ScalarTypes.String)));
            functions.Add(Functions.UrlDecode, new ScalarFunctionInfo(new ScalarOverloadInfo(new UrlDecodeFunctionImpl(), ScalarTypes.String, ScalarTypes.String)));
        }

        public static ScalarOverloadInfo GetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters)
        {
            if (!TryGetOverload(symbol, arguments, parameters, out var overload))
            {
                throw new NotImplementedException($"Function {symbol.Display} is not implemented for argument types ({string.Join(", ", arguments.Select(arg => arg.ResultType.Display))}).");
            }

            Debug.Assert(overload != null);
            return overload;
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
