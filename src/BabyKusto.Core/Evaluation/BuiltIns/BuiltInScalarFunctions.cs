// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using BabyKusto.Core.Evaluation.BuiltIns.Impl;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns;

internal static class BuiltInScalarFunctions
{
    private static readonly Dictionary<FunctionSymbol, ScalarFunctionInfo> functions = new();

    static BuiltInScalarFunctions()
    {
        RegisterAdditionalFunctionSymbols();
        functions.Add(Functions.Not,
            new ScalarFunctionInfo(NotFunctionImpl.Overload));

        functions.Add(
            Functions.IsNull,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new IsNullBoolFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Bool),
                new ScalarOverloadInfo(new IsNullIntFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Int),
                new ScalarOverloadInfo(new IsNullLongFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Long),
                new ScalarOverloadInfo(new IsNullDoubleFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Real),
                new ScalarOverloadInfo(new IsNullDateTimeFunctionImpl(), ScalarTypes.Bool, ScalarTypes.DateTime),
                new ScalarOverloadInfo(new IsNullTimeSpanFunctionImpl(), ScalarTypes.Bool, ScalarTypes.TimeSpan),
                IsNullStringFunctionImpl.Overload));
        functions.Add(Functions.IsEmpty,
            new ScalarFunctionInfo(IsEmptyFunctionImpl.Overload));

        functions.Add(
            Functions.MinOf,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new MinOfIntFunctionImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
                new ScalarOverloadInfo(new MinOfLongFunctionImpl(), ScalarTypes.Long, ScalarTypes.Long,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new MinOfDoubleFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real,
                    ScalarTypes.Real)));

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

            static void AddCoalesce(List<ScalarOverloadInfo> overloads, Func<IScalarFunctionImpl> factory,
                ScalarSymbol type)
            {
                var impl = factory();

                for (var numArgs = 2; numArgs <= 4; numArgs++)
                {
                    var argTypes = new ScalarSymbol[numArgs];
                    for (var i = 0; i < numArgs; i++)
                    {
                        argTypes[i] = type;
                    }

                    overloads.Add(new ScalarOverloadInfo(impl, type, argTypes));
                }
            }
        }

        functions.Add(Functions.Now,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new NowFunctionImpl(), ScalarTypes.DateTime)));
        functions.Add(Functions.Ago,
            new ScalarFunctionInfo(AgoFunctionImpl.Overload));

        // TODO: Support N-ary functions properly
        functions.Add(
            Functions.Strcat,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String),
                new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String,
                    ScalarTypes.String),
                new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String,
                    ScalarTypes.String, ScalarTypes.String),
                new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String,
                    ScalarTypes.String, ScalarTypes.String, ScalarTypes.String),
                new ScalarOverloadInfo(new StrcatFunctionImpl(), ScalarTypes.String, ScalarTypes.String,
                    ScalarTypes.String, ScalarTypes.String, ScalarTypes.String, ScalarTypes.String)));

        StrlenFunction.Register(functions);
        ToLowerFunction.Register(functions);
        ToUpperFunction.Register(functions);

        functions.Add(Functions.ReplaceString,
            new ScalarFunctionInfo(ReplaceStringFunctionImpl.Overload));

        functions.Add(
            Functions.Substring,
            new ScalarFunctionInfo(SubstringFunctionImpl.Overload));

        BinFunction.Register(functions);
        functions.Add(Functions.Floor, BinFunction.S);

        functions.Add(Functions.GetYear,
            new ScalarFunctionInfo(GetYearFunctionImpl.Overload));
        functions.Add(Functions.MonthOfYear,
            new ScalarFunctionInfo(GetMonthFunctionImpl.Overload));
        functions.Add(Functions.HourOfDay,
            new ScalarFunctionInfo(GetMonthFunctionImpl.Overload));


        functions.Add(Functions.Abs,
            new ScalarFunctionInfo(AbsFunctionImpl.Overload));
        functions.Add(Functions.Sin,
            new ScalarFunctionInfo(SinFunctionImpl.Overload));
        functions.Add(Functions.Cos,
            new ScalarFunctionInfo(CosFunctionImpl.Overload));
        functions.Add(Functions.Tan,
            new ScalarFunctionInfo(TanFunctionImpl.Overload));
        functions.Add(Functions.Sign,
            new ScalarFunctionInfo(SignFunctionImpl.Overload));
        functions.Add(Functions.Round,
            new ScalarFunctionInfo(RoundFunctionImpl.Overload));
        functions.Add(Functions.Radians,
            new ScalarFunctionInfo(RadiansFunctionImpl.Overload));
        functions.Add(Functions.Degrees,
            new ScalarFunctionInfo(DegreesFunctionImpl.Overload));

        functions.Add(Functions.Exp,
            new ScalarFunctionInfo(ExpFunctionImpl.Overload));
        functions.Add(Functions.Log,
            new ScalarFunctionInfo(LogFunctionImpl.Overload));
        functions.Add(Functions.Log10,
            new ScalarFunctionInfo(Log10FunctionImpl.Overload));
        functions.Add(Functions.Log2,
            new ScalarFunctionInfo(Log2FunctionImpl.Overload));
        functions.Add(Functions.Pow,
            new ScalarFunctionInfo(PowFunctionImpl.Overload));
        functions.Add(Functions.Sqrt,
            new ScalarFunctionInfo(SqrtFunctionImpl.Overload));

        functions.Add(Functions.DayOfWeek,
            new ScalarFunctionInfo(DayOfWeekFunctionImpl.Overload));
        functions.Add(Functions.DayOfMonth,
            new ScalarFunctionInfo(DayOfMonthFunctionImpl.Overload));
        functions.Add(Functions.DayOfYear,
            new ScalarFunctionInfo(DayOfYearFunctionImpl.Overload));
        functions.Add(Functions.StartOfDay,
            new ScalarFunctionInfo(StartOfDayFunctionImpl.Overload));
        functions.Add(Functions.EndOfDay,
            new ScalarFunctionInfo(EndOfDayFunctionImpl.Overload));
        functions.Add(Functions.StartOfWeek,
            new ScalarFunctionInfo(StartOfWeekFunctionImpl.Overload));
        functions.Add(Functions.EndOfWeek,
            new ScalarFunctionInfo(EndOfWeekFunctionImpl.Overload));
        functions.Add(Functions.StartOfMonth,
            new ScalarFunctionInfo(StartOfMonthFunctionImpl.Overload));
        functions.Add(Functions.EndOfMonth,
            new ScalarFunctionInfo(EndOfMonthFunctionImpl.Overload));
        functions.Add(Functions.StartOfYear,
            new ScalarFunctionInfo(StartOfYearFunctionImpl.Overload));
        functions.Add(Functions.EndOfYear,
            new ScalarFunctionInfo(EndOfYearFunctionImpl.Overload));

        functions.Add(Functions.DatetimeUtcToLocal,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new DateTimeUtcToLocalFunctionImpl(), ScalarTypes.DateTime,
                ScalarTypes.DateTime, ScalarTypes.String)));


        var iffFunctionInfo = new ScalarFunctionInfo(
            new ScalarOverloadInfo(new IffBoolFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Bool, ScalarTypes.Bool,
                ScalarTypes.Bool),
            new ScalarOverloadInfo(new IffIntFunctionImpl(), ScalarTypes.Int, ScalarTypes.Bool, ScalarTypes.Int,
                ScalarTypes.Int),
            new ScalarOverloadInfo(new IffLongFunctionImpl(), ScalarTypes.Long, ScalarTypes.Bool, ScalarTypes.Long,
                ScalarTypes.Long),
            new ScalarOverloadInfo(new IffRealFunctionImpl(), ScalarTypes.Real, ScalarTypes.Bool, ScalarTypes.Real,
                ScalarTypes.Real),
            new ScalarOverloadInfo(new IffDateTimeFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.Bool,
                ScalarTypes.DateTime, ScalarTypes.DateTime),
            new ScalarOverloadInfo(new IffTimeSpanFunctionImpl(), ScalarTypes.TimeSpan, ScalarTypes.Bool,
                ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
            new ScalarOverloadInfo(new IffStringFunctionImpl(), ScalarTypes.String, ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String));
        functions.Add(Functions.Iff, iffFunctionInfo);
        functions.Add(Functions.Iif, iffFunctionInfo);

        functions.Add(Functions.ToInt,
            new ScalarFunctionInfo(ToIntStringFunctionImpl.Overload));
        functions.Add(Functions.ToLong,
            new ScalarFunctionInfo(ToLongStringFunctionImpl.Overload));
        var toDoubleFunctionInfo = new ScalarFunctionInfo(ToDoubleStringFunctionImpl.Overload);
        functions.Add(Functions.ToReal, toDoubleFunctionInfo);
        functions.Add(Functions.ToDouble, toDoubleFunctionInfo);
        functions.Add(Functions.ToBool,
            new ScalarFunctionInfo(ToBoolStringFunctionImpl.Overload));

        functions.Add(Functions.ToString, new ScalarFunctionInfo(
            ToStringFromIntFunctionImpl.Overload,
            ToStringFromLongFunctionImpl.Overload,
            ToStringFromRealFunctionImpl.Overload,
            ToStringFromTimeSpanFunctionImpl.Overload,
            ToStringFromDateTimeFunctionImpl.Overload,
            new ScalarOverloadInfo(new ToStringFromDynamicFunctionImpl(), ScalarTypes.String, ScalarTypes.Dynamic),
            ToStringFromStringFunctionImpl.Overload));

        functions.Add(Functions.UrlEncode_Component,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new UrlEncodeComponentFunctionImpl(), ScalarTypes.String,
                ScalarTypes.String)));
        functions.Add(Functions.UrlDecode,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new UrlDecodeFunctionImpl(), ScalarTypes.String,
                ScalarTypes.String)));

        functions.Add(Functions.Extract,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new ExtractRegexFunctionImpl(), ScalarTypes.String,
                ScalarTypes.String, ScalarTypes.Long, ScalarTypes.String)));

        var rowNumberHints = EvaluationHints.ForceColumnarEvaluation | EvaluationHints.RequiresTableSerialization;
        functions.Add(Functions.RowNumber,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new RowNumberFunctionImpl(),
                    rowNumberHints,
                    ScalarTypes.Long),
                //variant that accepts initial index value as first item
                new ScalarOverloadInfo(new RowNumberFunctionImpl(),
                    rowNumberHints,
                    ScalarTypes.Long,
                    ScalarTypes.Long),
                //variant that accepts initial index value as first item
                //and flag to reset rank as second
                new ScalarOverloadInfo(new RowNumberFunctionImpl(),
                    rowNumberHints,
                    ScalarTypes.Long,
                    ScalarTypes.Long,
                    ScalarTypes.Bool)
            )
        );

        var randHints = EvaluationHints.ForceColumnarEvaluation | EvaluationHints.RequiresTableSerialization;
        functions.Add(Functions.Rand,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new RandFunctionImpl(),
                    randHints,
                    ScalarTypes.Real),
                new ScalarOverloadInfo(new RandFunctionImpl(),
                    randHints,
                    ScalarTypes.Real,
                    ScalarTypes.Long)
            )
        );


        functions.Add(Functions.ParseJson, new ScalarFunctionInfo(
            new ScalarOverloadInfo(new ParseJsonDynamicFunctionImpl(), ScalarTypes.Dynamic, ScalarTypes.Dynamic),
            new ScalarOverloadInfo(new ParseJsonStringFunctionImpl(), ScalarTypes.Dynamic, ScalarTypes.String)));

        functions.Add(Functions.ArraySortAsc, new ScalarFunctionInfo(
            new ScalarOverloadInfo(new ArraySortFunctionImpl(ascending: true), ScalarTypes.Dynamic,
                ScalarTypes.Dynamic)));
        functions.Add(Functions.ArraySortDesc, new ScalarFunctionInfo(
            new ScalarOverloadInfo(new ArraySortFunctionImpl(ascending: false), ScalarTypes.Dynamic,
                ScalarTypes.Dynamic)));

        functions.Add(Functions.ArrayLength, new ScalarFunctionInfo(
            new ScalarOverloadInfo(new ArrayLengthFunctionImpl(), ScalarTypes.Long, ScalarTypes.Dynamic)));

        functions.Add(Functions.GeoDistance2Points, new ScalarFunctionInfo(
            new ScalarOverloadInfo(new GeoDistance2PointsFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real,
                ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real)));

        functions.Add(Functions.GeoPointToGeohash, new ScalarFunctionInfo(
            new ScalarOverloadInfo(new GeoPointToGeoHashFunctionImpl(), ScalarTypes.String, ScalarTypes.Real,
                ScalarTypes.Real,
                ScalarTypes.Long),
            new ScalarOverloadInfo(new GeoPointToGeoHashFunctionImpl(), ScalarTypes.String, ScalarTypes.Real,
                ScalarTypes.Real)
        ));

        functions.Add(Functions.GeohashToCentralPoint, new ScalarFunctionInfo(
            new ScalarOverloadInfo(new GeoHashToCentralPointFunctionImpl(), ScalarTypes.Dynamic, ScalarTypes.String)
        ));


        functions.Add(
            Functions.Trim,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new TrimFunctionImpl(),
                    ScalarTypes.String,
                    ScalarTypes.String,
                    ScalarTypes.String)));
        functions.Add(
            Functions.TrimStart,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new TrimStartFunctionImpl(),
                    ScalarTypes.String,
                    ScalarTypes.String,
                    ScalarTypes.String)));

        functions.Add(
            Functions.TrimEnd,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new TrimEndFunctionImpl(),
                    ScalarTypes.String,
                    ScalarTypes.String,
                    ScalarTypes.String)));


        functions.Add(
            Functions.Split,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new SplitFunctionImpl(),
                    ScalarTypes.DynamicArrayOfString,
                    ScalarTypes.String,
                    ScalarTypes.String)));

        ScalarOverloadInfo[] BuildOverloads(IScalarFunctionImpl func, TypeSymbol t)
            => Enumerable.Range(1, 10)
                .Select(i =>
                {
                    var pairs = Enumerable.Range(0, i)
                        .SelectMany(_ => new[] { ScalarTypes.Bool, t })
                        .ToArray();
                    var paramArray = pairs.Append(t).ToArray();
                    var overload = new ScalarOverloadInfo(func,
                        t,
                        paramArray);
                    return overload;
                })
                .ToArray();

        functions.Add(
            Functions.Case,
            new ScalarFunctionInfo
            (
                //note most restrictive types have to be checked first!
                Array.Empty<ScalarOverloadInfo>()
                    .Concat(BuildOverloads(new CaseFunctionImpl<bool?>(), ScalarTypes.Bool))
                    .Concat(BuildOverloads(new CaseFunctionImpl<int?>(), ScalarTypes.Int))
                    .Concat(BuildOverloads(new CaseFunctionImpl<long?>(), ScalarTypes.Long))
                    .Concat(BuildOverloads(new CaseFunctionImpl<double?>(), ScalarTypes.Real))
                    .Concat(BuildOverloads(new CaseFunctionImpl<DateTime?>(), ScalarTypes.DateTime))
                    .Concat(BuildOverloads(new CaseFunctionImpl<string>(), ScalarTypes.String))
                    .ToArray()
            ));
        functions.Add(DebugEmit, new ScalarFunctionInfo(new ScalarOverloadInfo(new DebugEmitImpl(),
            ScalarTypes.Int,
            ScalarTypes.String)));
        functions.Add(Levenshtein, new ScalarFunctionInfo(LevenshteinDistanceImpl.Overload));
        functions.Add(StringSimilarity, new ScalarFunctionInfo(StringSimilarityImpl.Overload));

        functions.Add(DateTimeToIso, new ScalarFunctionInfo(DateTimeToIsoImpl.Overload));

        functions.Add(TrimWs, new ScalarFunctionInfo(TrimWsFunctionImpl.Overload));
    }

    /// <summary>
    ///     We don't do anything inside this function, however we need this so that the Kusto engine can ensure that the static
    ///     constructor is called and functions are registered
    /// </summary>
    public static void Initialize()
    {
    }

    private static void RegisterAdditionalFunctionSymbols()
    {
        var functions = BabyKustoEngine.GlobalStateInstance.Functions;
        var newFunctions = functions.Concat(AdditionalFunctionSymbols).ToArray();
        BabyKustoEngine.GlobalStateInstance = BabyKustoEngine.GlobalStateInstance.WithFunctions(newFunctions);
    }


    public static bool TryGetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters,
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

    #region AdditionalFunctionSymbols

    public static readonly FunctionSymbol DebugEmit =
        new FunctionSymbol("debug_emit", ScalarTypes.Int,
                new Parameter("value1", ScalarTypes.String)
            ).ConstantFoldable()
            .WithResultNameKind(ResultNameKind.None);


    public static readonly FunctionSymbol Levenshtein =
        new FunctionSymbol("levenshtein", ScalarTypes.Int, new Parameter("value1", ScalarTypes.String),
                new Parameter("value2", ScalarTypes.String)).ConstantFoldable()
            .WithResultNameKind(ResultNameKind.None);

    public static readonly FunctionSymbol StringSimilarity =
        new FunctionSymbol("string_similarity", ScalarTypes.Real, new Parameter("value1", ScalarTypes.String),
                new Parameter("value2", ScalarTypes.String)).ConstantFoldable()
            .WithResultNameKind(ResultNameKind.None);

    public static readonly FunctionSymbol DateTimeToIso =
        new FunctionSymbol("datetime_to_iso", ScalarTypes.String, new Parameter("value1", ScalarTypes.DateTime))
            .ConstantFoldable()
            .WithResultNameKind(ResultNameKind.None);

    public static readonly FunctionSymbol TrimWs =
        new FunctionSymbol("trimws", ScalarTypes.String, new Parameter("value1", ScalarTypes.String))
            .ConstantFoldable()
            .WithResultNameKind(ResultNameKind.None);

    public static readonly FunctionSymbol[] AdditionalFunctionSymbols =
        { DebugEmit, Levenshtein, StringSimilarity, DateTimeToIso, TrimWs };

    #endregion
}