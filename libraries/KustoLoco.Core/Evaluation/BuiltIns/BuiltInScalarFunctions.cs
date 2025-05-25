// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Kusto.Language.Symbols;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal static class BuiltInScalarFunctions
{
    internal static readonly Dictionary<FunctionSymbol, ScalarFunctionInfo> Functions = new();

    static BuiltInScalarFunctions()
    {
        NotFunction.Register(Functions);
        Functions.Add(
            Kusto.Language.Functions.IsNull,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new IsNullBoolFunctionImpl(), ScalarTypes.Bool,
                    ScalarTypes.Bool),
                new ScalarOverloadInfo(new IsNullIntFunctionImpl(), ScalarTypes.Bool,
                    ScalarTypes.Int),
                new ScalarOverloadInfo(new IsNullLongFunctionImpl(), ScalarTypes.Bool,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new IsNullDoubleFunctionImpl(), ScalarTypes.Bool,
                    ScalarTypes.Real),
                new ScalarOverloadInfo(new IsNullDecimalFunctionImpl(), ScalarTypes.Bool,
                    ScalarTypes.Decimal),
                new ScalarOverloadInfo(new IsNullDateTimeFunctionImpl(), ScalarTypes.Bool,
                    ScalarTypes.DateTime),
                new ScalarOverloadInfo(new IsNullTimeSpanFunctionImpl(), ScalarTypes.Bool,
                    ScalarTypes.TimeSpan),
                IsNullStringFunctionImpl.Overload));

        IsEmptyFunction.Register(Functions);
        IsNotEmptyFunction.Register(Functions);
        IsAsciiFunction.Register(Functions);
        IsFiniteFunction.Register(Functions);
        IsInfFunction.Register(Functions);
        IsNanFunction.Register(Functions);
        IsUtf8Function.Register(Functions);
        ReverseFunction.Register(Functions);
      
        {
            var overloads = new List<ScalarOverloadInfo>();

            AddCoalesce(overloads, () => new CoalesceBoolFunctionImpl(), ScalarTypes.Bool);
            AddCoalesce(overloads, () => new CoalesceIntFunctionImpl(), ScalarTypes.Int);
            AddCoalesce(overloads, () => new CoalesceLongFunctionImpl(), ScalarTypes.Long);
            AddCoalesce(overloads, () => new CoalesceDoubleFunctionImpl(), ScalarTypes.Real);
            AddCoalesce(overloads, () => new CoalesceDecimalFunctionImpl(), ScalarTypes.Decimal);
            AddCoalesce(overloads, () => new CoalesceDateTimeFunctionImpl(), ScalarTypes.DateTime);
            AddCoalesce(overloads, () => new CoalesceTimeSpanFunctionImpl(), ScalarTypes.TimeSpan);
            AddCoalesce(overloads, () => new CoalesceStringFunctionImpl(), ScalarTypes.String);

            Functions.Add(Kusto.Language.Functions.Coalesce, new ScalarFunctionInfo(overloads.ToArray()));

            static void AddCoalesce(List<ScalarOverloadInfo> overloads, Func<IScalarFunctionImpl> factory,
                ScalarSymbol type)
            {
                var impl = factory();
                // Coalesce can take up to 64 arguments but we limit it to 16 here
                for (var numArgs = 2; numArgs <= 16; numArgs++)
                {
                    var argTypes = new TypeSymbol[numArgs];
                    for (var i = 0; i < numArgs; i++) argTypes[i] = type;

                    overloads.Add(new ScalarOverloadInfo(impl, type, argTypes));
                }
            }
        }

        Functions.Add(Kusto.Language.Functions.Now,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new NowFunctionImpl(), ScalarTypes.DateTime)));
        Functions.Add(Kusto.Language.Functions.PI,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new PiFunctionImpl(),
                ScalarTypes.Real)));


        AgoFunction.Register(Functions);
        FormatDateTime.Register(Functions);

        //add multiple overloads for strcat
        var strcatOverrides = Enumerable.Range(1, 64)
            .Select(n =>
                new ScalarOverloadInfo(new StrcatFunctionImpl(),
                    ScalarTypes.String,
                    Enumerable.Range(0, n).Select(_ => (TypeSymbol)ScalarTypes.String).ToArray()))
            .ToArray();

        Functions.Add(Kusto.Language.Functions.Strcat, new ScalarFunctionInfo(strcatOverrides));

        //add multiple overloads for strcat_delim
        var strcatDelimiterOverrides = Enumerable.Range(2, 64)
            .Select(n =>
                new ScalarOverloadInfo(new StrcatDelimFunctionImpl(),
                    ScalarTypes.String,
                    Enumerable.Range(0, n).Select(TypeSymbol (_) => ScalarTypes.String).ToArray()))
            .ToArray();
       
        Functions.Add(Kusto.Language.Functions.StrcatDelim, new ScalarFunctionInfo(strcatDelimiterOverrides));
        AroundFunction.Register(Functions);
        StrlenFunction.Register(Functions);
        StrcmpFunction.Register(Functions);
        StrcatArrayFunction.Register(Functions);
        ArrayReverseFunction.Register(Functions);
        StrRepFunction.Register(Functions);
        MaxOfFunction.Register(Functions);
        MinOfFunction.Register(Functions);
        CountOfFunction.Register(Functions);
        IndexOfFunction.Register(Functions);
        ToLowerFunction.Register(Functions);
        ToUpperFunction.Register(Functions);
       
        ReplaceStringFunction.Register(Functions);
        SubstringFunction.Register(Functions);
        ArrayRotateLeftFunction.Register(Functions);
        ArrayRotateRightFunction.Register(Functions);

        Base64Decode.Register(Functions);
        Base64Encode.Register(Functions);
        BinaryAnd.Register(Functions);
        BinaryOr.Register(Functions);
        BinaryXor.Register(Functions);
        BinaryNot.Register(Functions);
        BinaryShiftLeft.Register(Functions);
        BinaryShiftRight.Register(Functions);
        BitsetCountOnes.Register(Functions);


        BinFunction.Register(Functions);
        Functions.Add(Kusto.Language.Functions.Floor, BinFunction.S);
        GetYearFunction.Register(Functions);
        GetMonthFunction.Register(Functions);
        GetHourOfDayFunction.Register(Functions);
        AbsFunction.Register(Functions);
        SinFunction.Register(Functions);
        CosFunction.Register(Functions);
        TanFunction.Register(Functions);
        SignFunction.Register(Functions);
        RoundFunction.Register(Functions);
        ToHexFunction.Register(Functions);
        RadiansFunction.Register(Functions);
        DegreesFunction.Register(Functions);
        LogFunction.Register(Functions);
        ExpFunction.Register(Functions);

        Log10Function.Register(Functions);
        Log2Function.Register(Functions);
        PowFunction.Register(Functions);
        SqrtFunction.Register(Functions);
        DayOfWeekFunction.Register(Functions);

        DayOfMonthFunction.Register(Functions);

        DayOfYearFunction.Register(Functions);
        StartOfDayFunction.Register(Functions);
        EndOfDayFunction.Register(Functions);

        StartOfWeekFunction.Register(Functions);

        EndOfWeekFunction.Register(Functions);
        StartOfMonthFunction.Register(Functions);
        EndOfMonthFunction.Register(Functions);
        StartOfYearFunction.Register(Functions);
        EndOfYearFunction.Register(Functions);
        DatetimeDiffFunction.Register(Functions);
        DatetimeAddFunction.Register(Functions);
        DatetimePartFunction.Register(Functions);
      

        MakeDateTime.Register(Functions);
        MakeTimeSpan.Register(Functions);
        DateTimeUtcToLocalFunction.Register(Functions);

        var iffFunctionInfo = new ScalarFunctionInfo(
            new ScalarOverloadInfo(new IffBoolFunctionImpl(), ScalarTypes.Bool,
                ScalarTypes.Bool, ScalarTypes.Bool,
                ScalarTypes.Bool),
            new ScalarOverloadInfo(new IffIntFunctionImpl(), ScalarTypes.Int,
                ScalarTypes.Bool, ScalarTypes.Int,
                ScalarTypes.Int),
            new ScalarOverloadInfo(new IffLongFunctionImpl(), ScalarTypes.Long,
                ScalarTypes.Bool, ScalarTypes.Long,
                ScalarTypes.Long),
            new ScalarOverloadInfo(new IffRealFunctionImpl(), ScalarTypes.Real,
                ScalarTypes.Bool, ScalarTypes.Real,
                ScalarTypes.Real),
            new ScalarOverloadInfo(new IffDateTimeFunctionImpl(),
                ScalarTypes.DateTime, ScalarTypes.Bool,
                ScalarTypes.DateTime, ScalarTypes.DateTime),
            new ScalarOverloadInfo(new IffTimeSpanFunctionImpl(),
                ScalarTypes.TimeSpan, ScalarTypes.Bool,
                ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
            new ScalarOverloadInfo(new IffStringFunctionImpl(),
                ScalarTypes.String, ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String));
        Functions.Add(Kusto.Language.Functions.Iff, iffFunctionInfo);
        Functions.Add(Kusto.Language.Functions.Iif, iffFunctionInfo);
        

        ToIntFunction.Register(Functions);
        ToLongFunction.Register(Functions);
        ToDoubleFunction.Register(Functions);
        ToDecimalFunction.Register(Functions);
        ToRealFunction.Register(Functions);
        ToBoolFunction.Register(Functions);
        ToGuidFunction.Register(Functions);
        ToStringFunction.Register(Functions);
        ToDateTimeFunction.Register(Functions);
        ToTimespanFunction.Register(Functions);


        Functions.Add(Kusto.Language.Functions.UrlEncode_Component,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new UrlEncodeComponentFunctionImpl(),
                ScalarTypes.String,
                ScalarTypes.String)));
        Functions.Add(Kusto.Language.Functions.UrlDecode,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new UrlDecodeFunctionImpl(), ScalarTypes.String,
                ScalarTypes.String)));
        ExtractFunction.Register(Functions);
        /*Functions.Add(Kusto.Language.Functions.Extract,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new ExtractRegexFunctionImpl(), ScalarTypes.String,
                ScalarTypes.String, ScalarTypes.Long,
                ScalarTypes.String)));
        */
        var rowNumberHints = EvaluationHints.ForceColumnarEvaluation | EvaluationHints.RequiresTableSerialization;
        Functions.Add(Kusto.Language.Functions.RowNumber,
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
        Functions.Add(Kusto.Language.Functions.Rand,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new RandFunctionImpl(),
                    randHints,
                    ScalarTypes.Real),
                new ScalarOverloadInfo(new RandFunctionImpl(),
                    randHints,
                    ScalarTypes.Real,
                    ScalarTypes.Long)
            )
        );


        Functions.Add(Kusto.Language.Functions.ParseJson, new ScalarFunctionInfo(
            new
                ScalarOverloadInfo(new ParseJsonDynamicFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.Dynamic),
            new
                ScalarOverloadInfo(new ParseJsonStringFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.String)));

        Functions.Add(Kusto.Language.Functions.ArraySortAsc, new ScalarFunctionInfo(
            new
                ScalarOverloadInfo(new ArraySortFunctionImpl(true),
                    ScalarTypes.Dynamic,
                    ScalarTypes.Dynamic)));
        Functions.Add(Kusto.Language.Functions.ArraySortDesc, new ScalarFunctionInfo(
            new
                ScalarOverloadInfo(new ArraySortFunctionImpl(false),
                    ScalarTypes.Dynamic,
                    ScalarTypes.Dynamic)));

        Functions.Add(Kusto.Language.Functions.ArrayLength, new ScalarFunctionInfo(
            new
                ScalarOverloadInfo(new ArrayLengthFunctionImpl(),
                    ScalarTypes.Long,
                    ScalarTypes.Dynamic)));

        Functions.Add(Kusto.Language.Functions.GeoDistance2Points, new ScalarFunctionInfo(
            new
                ScalarOverloadInfo(new GeoDistance2PointsFunctionImpl(),
                    ScalarTypes.Real,
                    ScalarTypes.Real,
                    ScalarTypes.Real,
                    ScalarTypes.Real,
                    ScalarTypes.Real)));

        Functions.Add(Kusto.Language.Functions.GeoPointToGeohash, new ScalarFunctionInfo(
            new
                ScalarOverloadInfo(new GeoPointToGeoHashFunctionImpl(),
                    ScalarTypes.String,
                    ScalarTypes.Real,
                    ScalarTypes.Real,
                    ScalarTypes.Long),
            new
                ScalarOverloadInfo(new GeoPointToGeoHashFunctionImpl(),
                    ScalarTypes.String,
                    ScalarTypes.Real,
                    ScalarTypes.Real)
        ));

        Functions.Add(Kusto.Language.Functions.GeohashToCentralPoint, new ScalarFunctionInfo(
            new
                ScalarOverloadInfo(new GeoHashToCentralPointFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.String)
        ));


        Functions.Add(
            Kusto.Language.Functions.Trim,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new TrimFunctionImpl(),
                    ScalarTypes.String,
                    ScalarTypes.String,
                    ScalarTypes.String)));
        Functions.Add(
            Kusto.Language.Functions.TrimStart,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new TrimStartFunctionImpl(),
                    ScalarTypes.String,
                    ScalarTypes.String,
                    ScalarTypes.String)));

        Functions.Add(
            Kusto.Language.Functions.TrimEnd,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new TrimEndFunctionImpl(),
                    ScalarTypes.String,
                    ScalarTypes.String,
                    ScalarTypes.String)));


        Functions.Add(
            Kusto.Language.Functions.Split,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new SplitFunctionImpl(),
                    ScalarTypes.DynamicArrayOfString,
                    ScalarTypes.String,
                    ScalarTypes.String)));

        ScalarOverloadInfo[] BuildOverloads(IScalarFunctionImpl func, TypeSymbol t)
        {
            return Enumerable.Range(1, 10)
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
        }

        Functions.Add(
            Kusto.Language.Functions.Case,
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
    }
}
