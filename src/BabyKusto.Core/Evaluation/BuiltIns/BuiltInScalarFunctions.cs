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
            new ScalarFunctionInfo(new ScalarOverloadInfo(new NotFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Bool)));

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
        functions.Add(Functions.IsEmpty,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new IsEmptyFunctionImpl(), ScalarTypes.Bool,
                ScalarTypes.String)));

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
            new ScalarFunctionInfo(new ScalarOverloadInfo(new AgoFunctionImpl(), ScalarTypes.DateTime,
                ScalarTypes.TimeSpan)));

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

        functions.Add(Functions.Strlen,
            new ScalarFunctionInfo(StrlenFunctionImpl.Overload));

        functions.Add(Functions.ToLower,
            new ScalarFunctionInfo(ToLowerFunctionImpl.Overload));

        functions.Add(Functions.ToUpper,
            new ScalarFunctionInfo(ToUpperFunctionImpl.Overload));

        functions.Add(Functions.ReplaceString,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new ReplaceStringFunctionImpl(), ScalarTypes.String,
                ScalarTypes.String, ScalarTypes.String, ScalarTypes.String)));

        // TODO: Signature should be `string substring(string, int, int)`. But const literals are evaluated as long's by default and we do not support narrowing at this time.
        functions.Add(
            Functions.Substring,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new SubstringFunctionImpl(), ScalarTypes.String, ScalarTypes.String,
                    ScalarTypes.Long, ScalarTypes.Long)));

        var binFunctionInfo = new ScalarFunctionInfo(
            new ScalarOverloadInfo(new BinIntFunctionImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
            new ScalarOverloadInfo(new BinLongFunctionImpl(), ScalarTypes.Long, ScalarTypes.Long, ScalarTypes.Long),
            new ScalarOverloadInfo(new BinDoubleFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real),
            new ScalarOverloadInfo(new BinDateTimeTimeSpanFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime,
                ScalarTypes.TimeSpan));
        functions.Add(Functions.Bin, binFunctionInfo);
        functions.Add(Functions.Floor, binFunctionInfo);

        functions.Add(Functions.Exp,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new ExpFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real)));
        functions.Add(Functions.Log,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new LogFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real)));
        functions.Add(Functions.Log10,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new Log10FunctionImpl(), ScalarTypes.Real,
                ScalarTypes.Real)));
        functions.Add(Functions.Log2,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new Log2FunctionImpl(), ScalarTypes.Real, ScalarTypes.Real)));
        functions.Add(Functions.Pow,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new PowFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real,
                ScalarTypes.Real)));
        functions.Add(Functions.Sqrt,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new SqrtFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real)));

        functions.Add(Functions.DayOfWeek,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new DayOfWeekFunctionImpl(), ScalarTypes.TimeSpan,
                ScalarTypes.DateTime)));
        functions.Add(Functions.DayOfMonth,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new DayOfMonthFunctionImpl(), ScalarTypes.Int,
                ScalarTypes.DateTime)));
        functions.Add(Functions.DayOfYear,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new DayOfYearFunctionImpl(), ScalarTypes.Int,
                ScalarTypes.DateTime)));
        functions.Add(Functions.StartOfDay,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new StartOfDayFunctionImpl(), ScalarTypes.DateTime,
                ScalarTypes.DateTime)));
        functions.Add(Functions.EndOfDay,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new EndOfDayFunctionImpl(), ScalarTypes.DateTime,
                ScalarTypes.DateTime)));
        functions.Add(Functions.StartOfWeek,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new StartOfWeekFunctionImpl(), ScalarTypes.DateTime,
                ScalarTypes.DateTime)));
        functions.Add(Functions.EndOfWeek,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new EndOfWeekFunctionImpl(), ScalarTypes.DateTime,
                ScalarTypes.DateTime)));
        functions.Add(Functions.StartOfMonth,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new StartOfMonthFunctionImpl(), ScalarTypes.DateTime,
                ScalarTypes.DateTime)));
        functions.Add(Functions.EndOfMonth,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new EndOfMonthFunctionImpl(), ScalarTypes.DateTime,
                ScalarTypes.DateTime)));
        functions.Add(Functions.StartOfYear,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new StartOfYearFunctionImpl(), ScalarTypes.DateTime,
                ScalarTypes.DateTime)));
        functions.Add(Functions.EndOfYear,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new EndOfYearFunctionImpl(), ScalarTypes.DateTime,
                ScalarTypes.DateTime)));

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
            new ScalarFunctionInfo(new ScalarOverloadInfo(new ToIntStringFunctionImpl(), ScalarTypes.Int,
                ScalarTypes.String)));
        functions.Add(Functions.ToLong,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new ToLongStringFunctionImpl(), ScalarTypes.Long,
                ScalarTypes.String)));
        var toDoubleFunctionInfo = new ScalarFunctionInfo(new ScalarOverloadInfo(new ToDoubleStringFunctionImpl(),
            ScalarTypes.Real, ScalarTypes.String));
        functions.Add(Functions.ToReal, toDoubleFunctionInfo);
        functions.Add(Functions.ToDouble, toDoubleFunctionInfo);
        functions.Add(Functions.ToBool,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new ToBoolStringFunctionImpl(), ScalarTypes.Bool,
                ScalarTypes.String)));

        functions.Add(Functions.ToString, new ScalarFunctionInfo(
            new ScalarOverloadInfo(new ToStringFromIntFunctionImpl(), ScalarTypes.String, ScalarTypes.Int),
            new ScalarOverloadInfo(new ToStringFromLongFunctionImpl(), ScalarTypes.String, ScalarTypes.Long),
            new ScalarOverloadInfo(new ToStringFromRealFunctionImpl(), ScalarTypes.String, ScalarTypes.Real),
            new ScalarOverloadInfo(new ToStringFromTimeSpanFunctionImpl(), ScalarTypes.String, ScalarTypes.TimeSpan),
            new ScalarOverloadInfo(new ToStringFromDateTimeFunctionImpl(), ScalarTypes.String, ScalarTypes.DateTime),
            new ScalarOverloadInfo(new ToStringFromDynamicFunctionImpl(), ScalarTypes.String, ScalarTypes.Dynamic),
            new ScalarOverloadInfo(new ToStringFromStringFunctionImpl(), ScalarTypes.String, ScalarTypes.String)));

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
        functions.Add(Levenshtein, new ScalarFunctionInfo(new ScalarOverloadInfo(new LevenshteinDistanceImpl(),
            ScalarTypes.Int,
            ScalarTypes.String,
            ScalarTypes.String)));
        functions.Add(StringSimilarity, new ScalarFunctionInfo(new ScalarOverloadInfo(new StringSimilarityImpl(),
            ScalarTypes.Real,
            ScalarTypes.String,
            ScalarTypes.String)));

        functions.Add(DateTimeToIso, new ScalarFunctionInfo(new ScalarOverloadInfo(new DateTimeToIsoImpl(),
            ScalarTypes.String,
            ScalarTypes.DateTime)));

        functions.Add(TrimWs, new ScalarFunctionInfo(new ScalarOverloadInfo(new TrimWsFunctionImpl(),
            ScalarTypes.String,
            ScalarTypes.String)));
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