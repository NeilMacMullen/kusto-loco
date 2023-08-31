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
            functions.Add(Functions.Not, new ScalarFunctionInfo(new ScalarOverloadInfo(new NotFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Bool)));

            functions.Add(
                Functions.IsNull,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new IsNullBoolFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Bool),
                    new ScalarOverloadInfo(new IsNullIntFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Int),
                    new ScalarOverloadInfo(new IsNullLongFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Long),
                    new ScalarOverloadInfo(new IsNullDoubleFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Real),
                    new ScalarOverloadInfo(new IsNullDateTimeFunctionImpl(), ScalarTypes.Bool, ScalarTypes.DateTime),
                    new ScalarOverloadInfo(new IsNullTimeSpanFunctionImpl(), ScalarTypes.Bool, ScalarTypes.TimeSpan),
                    new ScalarOverloadInfo(new IsNullStringFunctionImpl(), ScalarTypes.Bool, ScalarTypes.String)));
            functions.Add(Functions.IsEmpty, new ScalarFunctionInfo(new ScalarOverloadInfo(new IsEmptyFunctionImpl(), ScalarTypes.Bool, ScalarTypes.String)));

            functions.Add(
                Functions.MinOf,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new MinOfIntFunctionImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new MinOfLongFunctionImpl(), ScalarTypes.Long, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new MinOfDoubleFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real)));

            {
                var overloads = new List<ScalarOverloadInfo>();

                AddCoalesce(overloads, () => new CoalesceBoolFunctionImpl(), ScalarTypes.Bool);
                AddCoalesce(overloads, () => new CoalesceIntFunctionImpl(), ScalarTypes.Int);
                AddCoalesce(overloads, () => new CoalesceLongFunctionImpl(), ScalarTypes.Long);
                AddCoalesce(overloads, () => new CoalesceDoubleFunctionImpl(), ScalarTypes.Real);
                AddCoalesce(overloads, () => new CoalesceDateTimeFunctionImpl(), ScalarTypes.DateTime);
                AddCoalesce(overloads, () => new CoalesceTimeSpanFunctionImpl(), ScalarTypes.TimeSpan);
                AddCoalesce(overloads, () => new CoalesceStringFunctionImpl(), ScalarTypes.String);

                functions.Add(Functions.Coalesce, new ScalarFunctionInfo(overloads.ToArray()));

                static void AddCoalesce(List<ScalarOverloadInfo> overloads, Func<IScalarFunctionImpl> factory, ScalarSymbol type)
                {
                    var impl = factory();

                    for (int numArgs = 2; numArgs <= 4; numArgs++)
                    {
                        var argTypes = new ScalarSymbol[numArgs];
                        for (int i = 0; i < numArgs; i++)
                        {
                            argTypes[i] = type;
                        }

                        overloads.Add(new ScalarOverloadInfo(impl, type, argTypes));
                    }
                }
            }

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

            functions.Add(Functions.DayOfWeek, new ScalarFunctionInfo(new ScalarOverloadInfo(new DayOfWeekFunctionImpl(), ScalarTypes.TimeSpan, ScalarTypes.DateTime)));
            functions.Add(Functions.DayOfMonth, new ScalarFunctionInfo(new ScalarOverloadInfo(new DayOfMonthFunctionImpl(), ScalarTypes.Int, ScalarTypes.DateTime)));
            functions.Add(Functions.DayOfYear, new ScalarFunctionInfo(new ScalarOverloadInfo(new DayOfYearFunctionImpl(), ScalarTypes.Int, ScalarTypes.DateTime)));
            functions.Add(Functions.StartOfDay, new ScalarFunctionInfo(new ScalarOverloadInfo(new StartOfDayFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime)));
            functions.Add(Functions.EndOfDay, new ScalarFunctionInfo(new ScalarOverloadInfo(new EndOfDayFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime)));
            functions.Add(Functions.StartOfWeek, new ScalarFunctionInfo(new ScalarOverloadInfo(new StartOfWeekFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime)));
            functions.Add(Functions.EndOfWeek, new ScalarFunctionInfo(new ScalarOverloadInfo(new EndOfWeekFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime)));
            functions.Add(Functions.StartOfMonth, new ScalarFunctionInfo(new ScalarOverloadInfo(new StartOfMonthFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime)));
            functions.Add(Functions.EndOfMonth, new ScalarFunctionInfo(new ScalarOverloadInfo(new EndOfMonthFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime)));
            functions.Add(Functions.StartOfYear, new ScalarFunctionInfo(new ScalarOverloadInfo(new StartOfYearFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime)));
            functions.Add(Functions.EndOfYear, new ScalarFunctionInfo(new ScalarOverloadInfo(new EndOfYearFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime)));

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

            functions.Add(Functions.ToString, new ScalarFunctionInfo(
                new ScalarOverloadInfo(new ToStringFromIntFunctionImpl(), ScalarTypes.String, ScalarTypes.Int),
                new ScalarOverloadInfo(new ToStringFromLongFunctionImpl(), ScalarTypes.String, ScalarTypes.Long),
                new ScalarOverloadInfo(new ToStringFromRealFunctionImpl(), ScalarTypes.String, ScalarTypes.Real),
                new ScalarOverloadInfo(new ToStringFromTimeSpanFunctionImpl(), ScalarTypes.String, ScalarTypes.TimeSpan),
                new ScalarOverloadInfo(new ToStringFromDateTimeFunctionImpl(), ScalarTypes.String, ScalarTypes.DateTime),
                new ScalarOverloadInfo(new ToStringFromDynamicFunctionImpl(), ScalarTypes.String, ScalarTypes.Dynamic),
                new ScalarOverloadInfo(new ToStringFromStringFunctionImpl(), ScalarTypes.String, ScalarTypes.String)));

            functions.Add(Functions.UrlEncode_Component, new ScalarFunctionInfo(new ScalarOverloadInfo(new UrlEncodeComponentFunctionImpl(), ScalarTypes.String, ScalarTypes.String)));
            functions.Add(Functions.UrlDecode, new ScalarFunctionInfo(new ScalarOverloadInfo(new UrlDecodeFunctionImpl(), ScalarTypes.String, ScalarTypes.String)));

            functions.Add(Functions.Extract, new ScalarFunctionInfo(new ScalarOverloadInfo(new ExtractRegexFunctionImpl(), ScalarTypes.String, ScalarTypes.String, ScalarTypes.Long, ScalarTypes.String)));

            functions.Add(Functions.ParseJson, new ScalarFunctionInfo(
                new ScalarOverloadInfo(new ParseJsonDynamicFunctionImpl(), ScalarTypes.Dynamic, ScalarTypes.Dynamic),
                new ScalarOverloadInfo(new ParseJsonStringFunctionImpl(), ScalarTypes.Dynamic, ScalarTypes.String)));

            functions.Add(Functions.ArraySortAsc, new ScalarFunctionInfo(
                new ScalarOverloadInfo(new ArraySortFunctionImpl(ascending: true), ScalarTypes.Dynamic, ScalarTypes.Dynamic)));
            functions.Add(Functions.ArraySortDesc, new ScalarFunctionInfo(
                new ScalarOverloadInfo(new ArraySortFunctionImpl(ascending: false), ScalarTypes.Dynamic, ScalarTypes.Dynamic)));

            functions.Add(Functions.ArrayLength, new ScalarFunctionInfo(
                new ScalarOverloadInfo(new ArrayLengthFunctionImpl(), ScalarTypes.Long, ScalarTypes.Dynamic)));

            functions.Add(Functions.GeoDistance2Points, new ScalarFunctionInfo(
                new ScalarOverloadInfo(new GeoDistance2PointsFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real)));
        }

        public static ScalarOverloadInfo GetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters)
        {
            if (!TryGetOverload(symbol, arguments, parameters, out var overload))
            {
                throw new NotImplementedException($"Function {symbol.Name}{SchemaDisplay.GetText(symbol)} is not implemented for argument types ({string.Join(", ", arguments.Select(arg => SchemaDisplay.GetText(arg.ResultType)))}).");
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
