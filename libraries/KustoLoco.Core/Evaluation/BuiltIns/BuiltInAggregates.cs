//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kusto.Language;
using Kusto.Language.Symbols;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal static class BuiltInAggregates
{
    public static readonly Dictionary<FunctionSymbol, AggregateInfo> Aggregates = new();
    static BuiltInAggregates()
    {
        Aggregates.Add(Kusto.Language.Aggregates.Count,
            new AggregateInfo(new AggregateOverloadInfo(new CountFunctionImpl(), ScalarTypes.Long)));
        Aggregates.Add(Kusto.Language.Aggregates.CountIf,
            new AggregateInfo(new AggregateOverloadInfo(new CountIfFunctionImpl(), ScalarTypes.Long,
                ScalarTypes.Bool)));
        Aggregates.Add(
            Kusto.Language.Aggregates.DCount,
            new AggregateInfo(
                new AggregateOverloadInfo(new DCountAggregateIntImpl(), ScalarTypes.Long,
                    ScalarTypes.Int),
                new AggregateOverloadInfo(new DCountAggregateLongImpl(), ScalarTypes.Long,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new DCountAggregateDoubleImpl(), ScalarTypes.Long,
                    ScalarTypes.Real),
                new AggregateOverloadInfo(new DCountAggregateDecimalImpl(), ScalarTypes.Long,
                    ScalarTypes.Decimal),
                new AggregateOverloadInfo(new DCountAggregateDateTimeImpl(), ScalarTypes.Long,
                    ScalarTypes.DateTime),
                new AggregateOverloadInfo(new DCountAggregateTimeSpanImpl(), ScalarTypes.Long,
                    ScalarTypes.TimeSpan),
                new AggregateOverloadInfo(new DCountAggregateStringImpl(), ScalarTypes.Long,
                    ScalarTypes.String)));
        Aggregates.Add(
            Kusto.Language.Aggregates.CountDistinct,
            new AggregateInfo(
                new AggregateOverloadInfo(new DCountAggregateIntImpl(), ScalarTypes.Long,
                    ScalarTypes.Int),
                new AggregateOverloadInfo(new DCountAggregateLongImpl(), ScalarTypes.Long,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new DCountAggregateDoubleImpl(), ScalarTypes.Long,
                    ScalarTypes.Real),
                new AggregateOverloadInfo(new DCountAggregateDecimalImpl(), ScalarTypes.Long,
                    ScalarTypes.Decimal),
                new AggregateOverloadInfo(new DCountAggregateDateTimeImpl(), ScalarTypes.Long,
                    ScalarTypes.DateTime),
                new AggregateOverloadInfo(new DCountAggregateTimeSpanImpl(), ScalarTypes.Long,
                    ScalarTypes.TimeSpan),
                new AggregateOverloadInfo(new DCountAggregateStringImpl(), ScalarTypes.Long,
                    ScalarTypes.String)));
        Aggregates.Add(
            Kusto.Language.Aggregates.DCountIf,
            new AggregateInfo(
                new AggregateOverloadInfo(new DCountIfAggregateIntImpl(), ScalarTypes.Long,
                    ScalarTypes.Int,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new DCountIfAggregateLongImpl(), ScalarTypes.Long,
                    ScalarTypes.Long,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new DCountIfAggregateDoubleImpl(), ScalarTypes.Long,
                    ScalarTypes.Real,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new DCountIfAggregateDecimalImpl(), ScalarTypes.Long,
                    ScalarTypes.Decimal,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new DCountIfAggregateDateTimeImpl(),
                    ScalarTypes.Long, ScalarTypes.DateTime,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new DCountIfAggregateTimeSpanImpl(),
                    ScalarTypes.Long, ScalarTypes.TimeSpan,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new DCountIfAggregateStringImpl(), ScalarTypes.Long,
                    ScalarTypes.String,
                    ScalarTypes.Bool)));
        Aggregates.Add(
            Kusto.Language.Aggregates.CountDistinctIf,
            new AggregateInfo(
                new AggregateOverloadInfo(new DCountIfAggregateIntImpl(), ScalarTypes.Long,
                    ScalarTypes.Int,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new DCountIfAggregateLongImpl(), ScalarTypes.Long,
                    ScalarTypes.Long,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new DCountIfAggregateDoubleImpl(), ScalarTypes.Long,
                    ScalarTypes.Real,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new DCountIfAggregateDecimalImpl(), ScalarTypes.Long,
                    ScalarTypes.Decimal,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new DCountIfAggregateDateTimeImpl(),
                    ScalarTypes.Long, ScalarTypes.DateTime,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new DCountIfAggregateTimeSpanImpl(),
                    ScalarTypes.Long, ScalarTypes.TimeSpan,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new DCountIfAggregateStringImpl(), ScalarTypes.Long,
                    ScalarTypes.String,
                    ScalarTypes.Bool)));

        AvgAggregate.Register(Aggregates);
        AvgIfAggregate.Register(Aggregates);
        SumAggregate.Register(Aggregates);
        SumIfAggregate.Register(Aggregates);
        MinAggregate.Register(Aggregates);
        MinIfAggregate.Register(Aggregates);
        MaxAggregate.Register(Aggregates);
        MaxIfAggregate.Register(Aggregates);

        TakeAny.Register(Aggregates);

        Aggregates.Add(
            Kusto.Language.Aggregates.Percentile,
            new AggregateInfo(
                new AggregateOverloadInfo(new PercentileAggregateIntImpl(), ScalarTypes.Long,
                    ScalarTypes.Int,
                    ScalarTypes.Real),
                new AggregateOverloadInfo(new PercentileAggregateLongImpl(), ScalarTypes.Long,
                    ScalarTypes.Long,
                    ScalarTypes.Real),
                new AggregateOverloadInfo(new PercentileAggregateDoubleImpl(),
                    ScalarTypes.Real, ScalarTypes.Real,
                    ScalarTypes.Real)));
        
        Aggregates.Add(Kusto.Language.Aggregates.ArgMax,new AggregateInfo(
            new AggregateOverloadInfo(new ArgMaxFunctionIntImpl(),ScalarTypes.Dynamic,1,ScalarTypes.Int),
                new AggregateOverloadInfo(new ArgMaxFunctionLongImpl(), ScalarTypes.Dynamic,1, ScalarTypes.Long),
        new AggregateOverloadInfo(new ArgMaxFunctionDecimalImpl(), ScalarTypes.Dynamic,1, ScalarTypes.Decimal),
        new AggregateOverloadInfo(new ArgMaxFunctionDoubleImpl(), ScalarTypes.Dynamic,1, ScalarTypes.Real),
        new AggregateOverloadInfo(new ArgMaxFunctionDateTimeImpl(), ScalarTypes.Dynamic, 1, ScalarTypes.DateTime),
        new AggregateOverloadInfo(new ArgMaxFunctionTimeSpanImpl(), ScalarTypes.Dynamic, 1, ScalarTypes.TimeSpan)
            ));

        Aggregates.Add(Kusto.Language.Aggregates.ArgMin, new AggregateInfo(
            new AggregateOverloadInfo(new ArgMinFunctionIntImpl(), ScalarTypes.Dynamic, 1, ScalarTypes.Int),
            new AggregateOverloadInfo(new ArgMinFunctionLongImpl(), ScalarTypes.Dynamic, 1, ScalarTypes.Long),
            new AggregateOverloadInfo(new ArgMinFunctionDecimalImpl(), ScalarTypes.Dynamic, 1, ScalarTypes.Decimal),
            new AggregateOverloadInfo(new ArgMinFunctionDoubleImpl(), ScalarTypes.Dynamic, 1, ScalarTypes.Real),
            new AggregateOverloadInfo(new ArgMinFunctionDateTimeImpl(), ScalarTypes.Dynamic, 1, ScalarTypes.DateTime),
            new AggregateOverloadInfo(new ArgMinFunctionTimeSpanImpl(), ScalarTypes.Dynamic, 1, ScalarTypes.TimeSpan)
        ));

        Aggregates.Add(
            Kusto.Language.Aggregates.MakeSet,
            new AggregateInfo(
                new AggregateOverloadInfo(new MakeSetIntFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Int),
                new AggregateOverloadInfo(new MakeSetIntFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Int,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetLongFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetLongFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Long,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetDoubleFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Real),
                new AggregateOverloadInfo(new MakeSetDoubleFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Real,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetDecimalFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Decimal),
                new AggregateOverloadInfo(new MakeSetDecimalFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Decimal,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetTimeSpanFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.TimeSpan),
                new AggregateOverloadInfo(new MakeSetTimeSpanFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.TimeSpan,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetDateTimeFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.DateTime),
                new AggregateOverloadInfo(new MakeSetDateTimeFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.DateTime,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetStringFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.String),
                new AggregateOverloadInfo(new MakeSetStringFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.String,
                    ScalarTypes.Long)));

        Aggregates.Add(
            Kusto.Language.Aggregates.MakeSetIf,
            new AggregateInfo(
                new AggregateOverloadInfo(new MakeSetIfIntFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Int,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeSetIfIntFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Int,
                    ScalarTypes.Bool, ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetIfLongFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Long,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeSetIfLongFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Long,
                    ScalarTypes.Bool, ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetIfDoubleFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Real,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeSetIfDoubleFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Real,
                    ScalarTypes.Bool, ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetIfDecimalFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Decimal,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeSetIfDecimalFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Decimal,
                    ScalarTypes.Bool, ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetIfTimeSpanFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.TimeSpan, ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeSetIfTimeSpanFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.TimeSpan, ScalarTypes.Bool,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetIfDateTimeFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.DateTime, ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeSetIfDateTimeFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.DateTime, ScalarTypes.Bool,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeSetIfStringFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.String,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeSetIfStringFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.String,
                    ScalarTypes.Bool, ScalarTypes.Long)));

        Aggregates.Add(
            Kusto.Language.Aggregates.MakeList,
            new AggregateInfo(
                new AggregateOverloadInfo(new MakeListIntFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Int),
                new AggregateOverloadInfo(new MakeListIntFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Int,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListLongFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListLongFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Long,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListDoubleFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Real),
                new AggregateOverloadInfo(new MakeListDoubleFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Real,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListDecimalFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Decimal),
                new AggregateOverloadInfo(new MakeListDecimalFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Decimal,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListTimeSpanFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.TimeSpan),
                new AggregateOverloadInfo(new MakeListTimeSpanFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.TimeSpan,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListDateTimeFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.DateTime),
                new AggregateOverloadInfo(new MakeListDateTimeFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.DateTime,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListDynamicFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.Dynamic),
                new AggregateOverloadInfo(new MakeListDynamicFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Dynamic,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListStringFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.String),
                new AggregateOverloadInfo(new MakeListStringFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.String,
                    ScalarTypes.Long)));

        Aggregates.Add(
            Kusto.Language.Aggregates.MakeListIf,
            new AggregateInfo(
                new AggregateOverloadInfo(new MakeListIfIntFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Int,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeListIfIntFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Int,
                    ScalarTypes.Bool, ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListIfLongFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Long,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeListIfLongFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Long,
                    ScalarTypes.Bool, ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListIfDoubleFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Real,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeListIfDoubleFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Real,
                    ScalarTypes.Bool, ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListIfDecimalFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Decimal,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeListIfDecimalFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Decimal,
                    ScalarTypes.Bool, ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListIfTimeSpanFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.TimeSpan, ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeListIfTimeSpanFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.TimeSpan, ScalarTypes.Bool,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListIfDateTimeFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.DateTime, ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeListIfDateTimeFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.DateTime, ScalarTypes.Bool,
                    ScalarTypes.Long),

                new AggregateOverloadInfo(new MakeListIfDynamicFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.Dynamic, ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeListIfDynamicFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.Dynamic, ScalarTypes.Bool,
                    ScalarTypes.Long),
                
                new AggregateOverloadInfo(new MakeListIfStringFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.String,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeListIfStringFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.String,
                    ScalarTypes.Bool, ScalarTypes.Long)));

        Aggregates.Add(
            Kusto.Language.Aggregates.MakeListWithNulls,
            new AggregateInfo(
                new AggregateOverloadInfo(new MakeListWithNullsIntFunctionImpl(),
                    ScalarTypes.Dynamic, ScalarTypes.Int),
                new AggregateOverloadInfo(new MakeListWithNullsLongFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.Long),
                new AggregateOverloadInfo(new MakeListWithNullsDoubleFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.Real),
                new AggregateOverloadInfo(new MakeListWithNullsDecimalFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.Decimal),
                new AggregateOverloadInfo(new MakeListWithNullsTimeSpanFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.TimeSpan),
                new AggregateOverloadInfo(new MakeListWithNullsDateTimeFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.DateTime),
                new AggregateOverloadInfo(new MakeListWithNullsStringFunctionImpl(),
                    ScalarTypes.Dynamic,
                    ScalarTypes.String)));

        Aggregates.Add(
            Kusto.Language.Aggregates.BuildSchema,
            new AggregateInfo(
                new AggregateOverloadInfo(new BuildSchemaFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Dynamic)));

        Aggregates.Add(
            Kusto.Language.Aggregates.MakeBag,
            new AggregateInfo(
                new AggregateOverloadInfo(new MakeBagFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Dynamic),
                new AggregateOverloadInfo(new MakeBagFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Dynamic,
                    ScalarTypes.Long)));

        Aggregates.Add(
            Kusto.Language.Aggregates.MakeBagIf,
            new AggregateInfo(
                new AggregateOverloadInfo(new MakeBagIfFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Dynamic,
                    ScalarTypes.Bool),
                new AggregateOverloadInfo(new MakeBagIfFunctionImpl(), ScalarTypes.Dynamic,
                    ScalarTypes.Dynamic,
                    ScalarTypes.Bool, ScalarTypes.Long)));
    }

    public static AggregateOverloadInfo GetOverload(FunctionSymbol symbol,
        TypeSymbol returnType, IRExpressionNode[] arguments,
        List<Parameter> parameters)
    {
        if (!TryGetOverload(symbol, returnType, arguments, parameters, out var overload))
            throw new NotImplementedException(
                $"Aggregate function {symbol.Name}{SchemaDisplay.GetText(symbol)} is not implemented for argument types ({string.Join(", ", arguments.Select(arg => SchemaDisplay.GetText(arg.ResultType)))}).");

        return overload!;
    }

    public static bool TryGetOverload(FunctionSymbol symbol,
        TypeSymbol returnType, IRExpressionNode[] arguments, List<Parameter> parameters,
        out AggregateOverloadInfo? overload)
    {
        if (!Aggregates.TryGetValue(symbol, out var aggregateInfo))
        {
            overload = null;
            return false;
        }
        overload = BuiltInsHelper.PickOverload(returnType, aggregateInfo.Overloads, arguments);
        return overload != null;
    }
}
