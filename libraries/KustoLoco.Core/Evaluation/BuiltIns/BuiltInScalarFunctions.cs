//
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
        //not worth auto-generating these because they are short-circuited scalars
        Functions.Add(Kusto.Language.Functions.Now,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new NowFunctionImpl(), ScalarTypes.DateTime)));
        //not worth auto-generating these because they are short-circuited scalars

        Functions.Add(Kusto.Language.Functions.PI,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new PiFunctionImpl(),
                ScalarTypes.Real)));

        var forceColumnarHints = EvaluationHints.ForceColumnarEvaluation | EvaluationHints.RequiresTableSerialization;

        Functions.Add(Kusto.Language.Functions.Rand,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new RandFunctionImpl(),
                    forceColumnarHints,
                    ScalarTypes.Real),
                new ScalarOverloadInfo(new RandFunctionImpl(),
                    forceColumnarHints,
                    ScalarTypes.Real,
                    ScalarTypes.Real)
            )
        );

        Functions.Add(Kusto.Language.Functions.NewGuid,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new NewGuidFunctionImpl(),
                forceColumnarHints,
                ScalarTypes.Guid)
            )
        );


        DiagTicksFunction.Register(Functions);

        TicksFunction.Register(Functions);
        TidFunction.Register(Functions);
        ProcFunction.Register(Functions);
        DiagnosticsFunction.Register(Functions);

        //can't generate because arbitrary number of arguments
        Functions.Add(Kusto.Language.Functions.Strcat, new ScalarFunctionInfo(new ScalarOverloadInfo(
            new StrcatFunctionImpl(), true,
            ScalarTypes.String, ScalarTypes.String)));


        //can't generate because arbitrary number of arguments
        //add multiple overloads for strcat_delim
        var strcatDelimiterOverrides = Enumerable.Range(2, 64)
            .Select(n =>
                new ScalarOverloadInfo(new StrcatDelimFunctionImpl(),
                    ScalarTypes.String,
                    Enumerable.Range(0, n).Select(TypeSymbol (_) => ScalarTypes.String).ToArray()))
            .ToArray();

        Functions.Add(Kusto.Language.Functions.StrcatDelim, new ScalarFunctionInfo(strcatDelimiterOverrides));


        ScalarOverloadInfo[] MakePrevNextOverloads(IScalarFunctionImpl func, TypeSymbol type)
        {
            return
            [
                new ScalarOverloadInfo(func, type, type),
                new ScalarOverloadInfo(func, type, type, ScalarTypes.Long),
                new ScalarOverloadInfo(func, type, type, ScalarTypes.Long, type)
            ];
        }


        Functions.Add(Kusto.Language.Functions.Prev,
            new ScalarFunctionInfo(
                MakePrevNextOverloads(new PrevFunctionIntImpl(), ScalarTypes.Int)
                    .Concat(MakePrevNextOverloads(new PrevFunctionBoolImpl(), ScalarTypes.Bool))
                    .Concat(MakePrevNextOverloads(new PrevFunctionLongImpl(), ScalarTypes.Long))
                    .Concat(MakePrevNextOverloads(new PrevFunctionRealImpl(), ScalarTypes.Real))
                    .Concat(MakePrevNextOverloads(new PrevFunctionDecimalImpl(), ScalarTypes.Decimal))
                    .Concat(MakePrevNextOverloads(new PrevFunctionGuidImpl(), ScalarTypes.Guid))
                    .Concat(MakePrevNextOverloads(new PrevFunctionStringImpl(), ScalarTypes.String))
                    .Concat(MakePrevNextOverloads(new PrevFunctionDateTimeImpl(), ScalarTypes.DateTime))
                    .Concat(MakePrevNextOverloads(new PrevFunctionTimespanImpl(), ScalarTypes.TimeSpan))
                    .ToArray()
            )
        );

        Functions.Add(Kusto.Language.Functions.Next,
            new ScalarFunctionInfo(
                MakePrevNextOverloads(new NextFunctionIntImpl(), ScalarTypes.Int)
                    .Concat(MakePrevNextOverloads(new NextFunctionBoolImpl(), ScalarTypes.Bool))
                    .Concat(MakePrevNextOverloads(new NextFunctionLongImpl(), ScalarTypes.Long))
                    .Concat(MakePrevNextOverloads(new NextFunctionRealImpl(), ScalarTypes.Real))
                    .Concat(MakePrevNextOverloads(new NextFunctionDecimalImpl(), ScalarTypes.Decimal))
                    .Concat(MakePrevNextOverloads(new NextFunctionGuidImpl(), ScalarTypes.Guid))
                    .Concat(MakePrevNextOverloads(new NextFunctionStringImpl(), ScalarTypes.String))
                    .Concat(MakePrevNextOverloads(new NextFunctionDateTimeImpl(), ScalarTypes.DateTime))
                    .Concat(MakePrevNextOverloads(new NextFunctionTimespanImpl(), ScalarTypes.TimeSpan))
                    .ToArray()
            )
        );


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


        Functions.Add(
            Kusto.Language.Functions.Split,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new SplitFunctionImpl(),
                    ScalarTypes.DynamicArrayOfString,
                    ScalarTypes.String,
                    ScalarTypes.String),
                new ScalarOverloadInfo(new SplitWithIndexFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.String,
                    ScalarTypes.String,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new SplitWithIndexFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.Dynamic,
                    ScalarTypes.String,
                    ScalarTypes.Dynamic)));

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
                    .Concat(BuildOverloads(new GenericCaseFunctionImplOfbool(), ScalarTypes.Bool))
                    .Concat(BuildOverloads(new GenericCaseFunctionImplOfint(), ScalarTypes.Int))
                    .Concat(BuildOverloads(new GenericCaseFunctionImplOflong(), ScalarTypes.Long))
                    .Concat(BuildOverloads(new GenericCaseFunctionImplOfdecimal(), ScalarTypes.Decimal))
                    .Concat(BuildOverloads(new GenericCaseFunctionImplOfdouble(), ScalarTypes.Real))
                    .Concat(BuildOverloads(new GenericCaseFunctionImplOfDateTime(), ScalarTypes.DateTime))
                    .Concat(BuildOverloads(new GenericCaseFunctionImplOfTimeSpan(), ScalarTypes.TimeSpan))
                    .Concat(BuildOverloads(new GenericCaseFunctionImplOfstring(), ScalarTypes.String))
                    .Concat(BuildOverloads(new GenericCaseFunctionImplOfGuid(), ScalarTypes.Guid))
                    .Concat(BuildOverloads(new GenericCaseFunctionImplOfJsonNode(), ScalarTypes.Dynamic))
                    .ToArray()
            ));

        NotFunction.Register(Functions);
        IsNullFunction.Register(Functions);
        IsNotNullFunction.Register(Functions);

        IffFunction.Register(Functions);
        Functions.Add(Kusto.Language.Functions.Iif, IffFunction.S); //synonym for Iff
        TrimFunction.Register(Functions);
        TrimStartFunction.Register(Functions);
        TrimEndFunction.Register(Functions);
        IsEmptyFunction.Register(Functions);
        IsNotEmptyFunction.Register(Functions);
        Ipv4IsPrivateFunction.Register(Functions);
        IsAsciiFunction.Register(Functions);
        IsFiniteFunction.Register(Functions);
        IsInfFunction.Register(Functions);
        IsNanFunction.Register(Functions);
        IsUtf8Function.Register(Functions);
        ReverseFunction.Register(Functions);
        Coalesce.Register(Functions);
        AgoFunction.Register(Functions);
        FormatDateTime.Register(Functions);
        MinOfRegister.Register(Functions);
        MaxOfRegister.Register(Functions);
        AroundFunction.Register(Functions);
        StrlenFunction.Register(Functions);
        StrcmpFunction.Register(Functions);
        StrcatArrayFunction.Register(Functions);
        ArrayConcatFunction.Register(Functions);
        ArrayReverseFunction.Register(Functions);
        StrRepFunction.Register(Functions);
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
        BinAtFunction.Register(Functions);
        Functions.Add(Kusto.Language.Functions.Floor, BinFunction.S); //Floor is just a synonym for bin
        GetYearFunction.Register(Functions);
        GetMonthFunction.Register(Functions);
        GetHourOfDayFunction.Register(Functions);
        AbsFunction.Register(Functions);
        SinFunction.Register(Functions);
        CosFunction.Register(Functions);
        TanFunction.Register(Functions);
        CotFunction.Register(Functions);
        AsinFunction.Register(Functions);
        AcosFunction.Register(Functions);
        AtanFunction.Register(Functions);
        Atan2Function.Register(Functions);
        CeilingFunction.Register(Functions);
        SignFunction.Register(Functions);
        RoundFunction.Register(Functions);
        ToHexFunction.Register(Functions);
        RadiansFunction.Register(Functions);
        DegreesFunction.Register(Functions);
        LogFunction.Register(Functions);
        ExpFunction.Register(Functions);
        Exp2Function.Register(Functions);
        Exp10Function.Register(Functions);
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
        DateTimeLocalToUtcFunction.Register(Functions);
        ToIntFunction.Register(Functions);
        ToLongFunction.Register(Functions);
        ToDoubleFunction.Register(Functions);
        ToDecimalFunction.Register(Functions);
        ToRealFunction.Register(Functions);
        ToBoolFunction.Register(Functions);
        StringSizeFunction.Register(Functions);
        ToGuidFunction.Register(Functions);
        ToStringFunction.Register(Functions);
        ToDateTimeFunction.Register(Functions);
        ToTimespanFunction.Register(Functions);
        UrlDecodeFunction.Register(Functions);
        UrlEncodeComponentFunction.Register(Functions);
        ExtractFunction.Register(Functions);
        ParseJsonFunction.Register(Functions);
        ArraySortAscFunction.Register(Functions);
        ArraySortDescFunction.Register(Functions);
        ArrayLengthFunction.Register(Functions);
        GeoDistance2PointsFunction.Register(Functions);
        GeoPointToGeoHashFunction.Register(Functions);
        GeoHashToCentralPointFunction.Register(Functions);
        BenchmarkFunction.Register(Functions);
        ParTest.Register(Functions);
        DateTimeKindFunction.Register(Functions);
        ArrayIifFunction.Register(Functions);
        ArrayIffFunction.Register(Functions);
        ArrayIndexOfFunction.Register(Functions);
        ArraySliceFunction.Register(Functions);
        ArraySplitFunction.Register(Functions);
        ArraySumFunction.Register(Functions);
        UnixTimeSecondsToDateTimeFunction.Register(Functions);
        UnixTimeMilliSecondsToDateTimeFunction.Register(Functions);
        UnixTimeMicroSecondsToDateTimeFunction.Register(Functions);
        UnixTimeNanoSecondsToDateTimeFunction.Register(Functions);
        UrlEncodeFunction.Register(Functions);
        RegexQuoteFunction.Register(Functions);
        RepeatFunction.Register(Functions);
    }
}
