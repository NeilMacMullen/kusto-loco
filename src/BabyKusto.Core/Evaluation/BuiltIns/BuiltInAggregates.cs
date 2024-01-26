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

namespace BabyKusto.Core.Evaluation.BuiltIns;

internal static class BuiltInAggregates
{
    private static readonly Dictionary<FunctionSymbol, AggregateInfo> aggregates = new();

    static BuiltInAggregates()
    {
        aggregates.Add(Aggregates.Count,
                       new AggregateInfo(new AggregateOverloadInfo(new CountFunctionImpl(), ScalarTypes.Long)));
        aggregates.Add(Aggregates.CountIf,
                       new AggregateInfo(new AggregateOverloadInfo(new CountIfFunctionImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.Bool)));
        aggregates.Add(
                       Aggregates.DCount,
                       new AggregateInfo(
                                         new AggregateOverloadInfo(new DCountAggregateIntImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.Int),
                                         new AggregateOverloadInfo(new DCountAggregateLongImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.Long),
                                         new AggregateOverloadInfo(new DCountAggregateDoubleImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.Real),
                                         new AggregateOverloadInfo(new DCountAggregateDateTimeImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.DateTime),
                                         new AggregateOverloadInfo(new DCountAggregateTimeSpanImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.TimeSpan),
                                         new AggregateOverloadInfo(new DCountAggregateStringImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.String)));
        aggregates.Add(
                       Aggregates.DCountIf,
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
                                         new AggregateOverloadInfo(new DCountIfAggregateDateTimeImpl(),
                                                                   ScalarTypes.Long, ScalarTypes.DateTime,
                                                                   ScalarTypes.Bool),
                                         new AggregateOverloadInfo(new DCountIfAggregateTimeSpanImpl(),
                                                                   ScalarTypes.Long, ScalarTypes.TimeSpan,
                                                                   ScalarTypes.Bool),
                                         new AggregateOverloadInfo(new DCountIfAggregateStringImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.String,
                                                                   ScalarTypes.Bool)));

        aggregates.Add(
                       Aggregates.SumIf,
                       new AggregateInfo(
                                         new AggregateOverloadInfo(new SumIfIntFunctionImpl(), ScalarTypes.Int,
                                                                   ScalarTypes.Int,
                                                                   ScalarTypes.Bool),
                                         new AggregateOverloadInfo(new SumIfLongFunctionImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.Long,
                                                                   ScalarTypes.Bool),
                                         new AggregateOverloadInfo(new SumIfDoubleFunctionImpl(), ScalarTypes.Real,
                                                                   ScalarTypes.Real,
                                                                   ScalarTypes.Bool)));

        aggregates.Add(
                       Aggregates.Avg,
                       new AggregateInfo(
                                         new AggregateOverloadInfo(new AvgAggregateIntImpl(), ScalarTypes.Real,
                                                                   ScalarTypes.Int),
                                         new AggregateOverloadInfo(new AvgAggregateLongImpl(), ScalarTypes.Real,
                                                                   ScalarTypes.Long),
                                         new AggregateOverloadInfo(new AvgAggregateDoubleImpl(), ScalarTypes.Real,
                                                                   ScalarTypes.Real),
                                         new AggregateOverloadInfo(new AvgAggregateTimeSpanImpl(), ScalarTypes.TimeSpan,
                                                                   ScalarTypes.TimeSpan)
                                        ));

        aggregates.Add(
                       Aggregates.Sum,
                       new AggregateInfo(
                                         new AggregateOverloadInfo(new SumAggregateIntImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.Int),
                                         new AggregateOverloadInfo(new SumAggregateLongImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.Long),
                                         new AggregateOverloadInfo(new SumAggregateDoubleImpl(), ScalarTypes.Real,
                                                                   ScalarTypes.Real)));

        aggregates.Add(
                       Aggregates.Min,
                       new AggregateInfo(
                                         new AggregateOverloadInfo(new MinAggregateIntImpl(), ScalarTypes.Int,
                                                                   ScalarTypes.Int),
                                         new AggregateOverloadInfo(new MinAggregateLongImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.Long),
                                         new AggregateOverloadInfo(new MinAggregateDoubleImpl(), ScalarTypes.Real,
                                                                   ScalarTypes.Real),
                                         new AggregateOverloadInfo(new MinAggregateDateTimeImpl(), ScalarTypes.DateTime,
                                                                   ScalarTypes.DateTime),
                                         new AggregateOverloadInfo(new MinAggregateTimeSpanImpl(), ScalarTypes.TimeSpan,
                                                                   ScalarTypes.TimeSpan)));

        aggregates.Add(
                       Aggregates.Max,
                       new AggregateInfo(
                                         new AggregateOverloadInfo(new MaxAggregateIntImpl(), ScalarTypes.Int,
                                                                   ScalarTypes.Int),
                                         new AggregateOverloadInfo(new MaxAggregateLongImpl(), ScalarTypes.Long,
                                                                   ScalarTypes.Long),
                                         new AggregateOverloadInfo(new MaxAggregateDoubleImpl(), ScalarTypes.Real,
                                                                   ScalarTypes.Real),
                                         new AggregateOverloadInfo(new MaxAggregateDateTimeImpl(), ScalarTypes.DateTime,
                                                                   ScalarTypes.DateTime),
                                         new AggregateOverloadInfo(new MaxAggregateTimeSpanImpl(), ScalarTypes.TimeSpan,
                                                                   ScalarTypes.TimeSpan)));

        var takeAnyOverloads = new AggregateInfo(
                                                 new AggregateOverloadInfo(new TakeAnyFunctionImpl(), ScalarTypes.Bool,
                                                                           ScalarTypes.Bool),
                                                 new AggregateOverloadInfo(new TakeAnyFunctionImpl(), ScalarTypes.Int,
                                                                           ScalarTypes.Int),
                                                 new AggregateOverloadInfo(new TakeAnyFunctionImpl(), ScalarTypes.Long,
                                                                           ScalarTypes.Long),
                                                 new AggregateOverloadInfo(new TakeAnyFunctionImpl(), ScalarTypes.Real,
                                                                           ScalarTypes.Real),
                                                 new AggregateOverloadInfo(new TakeAnyFunctionImpl(),
                                                                           ScalarTypes.DateTime, ScalarTypes.DateTime),
                                                 new AggregateOverloadInfo(new TakeAnyFunctionImpl(),
                                                                           ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
                                                 new AggregateOverloadInfo(new TakeAnyFunctionImpl(),
                                                                           ScalarTypes.String, ScalarTypes.String));
        aggregates.Add(Aggregates.TakeAny, takeAnyOverloads);
        aggregates.Add(Aggregates.Any, takeAnyOverloads);

        aggregates.Add(
                       Aggregates.Percentile,
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

        aggregates.Add(
                       Aggregates.MakeSet,
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

        aggregates.Add(
                       Aggregates.MakeSetIf,
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

        aggregates.Add(
                       Aggregates.MakeList,
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
                                         new AggregateOverloadInfo(new MakeListStringFunctionImpl(),
                                                                   ScalarTypes.Dynamic, ScalarTypes.String),
                                         new AggregateOverloadInfo(new MakeListStringFunctionImpl(),
                                                                   ScalarTypes.Dynamic, ScalarTypes.String,
                                                                   ScalarTypes.Long)));

        aggregates.Add(
                       Aggregates.MakeListIf,
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
                                         new AggregateOverloadInfo(new MakeListIfStringFunctionImpl(),
                                                                   ScalarTypes.Dynamic, ScalarTypes.String,
                                                                   ScalarTypes.Bool),
                                         new AggregateOverloadInfo(new MakeListIfStringFunctionImpl(),
                                                                   ScalarTypes.Dynamic, ScalarTypes.String,
                                                                   ScalarTypes.Bool, ScalarTypes.Long)));

        aggregates.Add(
                       Aggregates.MakeListWithNulls,
                       new AggregateInfo(
                                         new AggregateOverloadInfo(new MakeListWithNullsIntFunctionImpl(),
                                                                   ScalarTypes.Dynamic, ScalarTypes.Int),
                                         new AggregateOverloadInfo(new MakeListWithNullsLongFunctionImpl(),
                                                                   ScalarTypes.Dynamic,
                                                                   ScalarTypes.Long),
                                         new AggregateOverloadInfo(new MakeListWithNullsDoubleFunctionImpl(),
                                                                   ScalarTypes.Dynamic,
                                                                   ScalarTypes.Real),
                                         new AggregateOverloadInfo(new MakeListWithNullsTimeSpanFunctionImpl(),
                                                                   ScalarTypes.Dynamic,
                                                                   ScalarTypes.TimeSpan),
                                         new AggregateOverloadInfo(new MakeListWithNullsDateTimeFunctionImpl(),
                                                                   ScalarTypes.Dynamic,
                                                                   ScalarTypes.DateTime),
                                         new AggregateOverloadInfo(new MakeListWithNullsStringFunctionImpl(),
                                                                   ScalarTypes.Dynamic,
                                                                   ScalarTypes.String)));
    }

    public static AggregateOverloadInfo GetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments,
        List<Parameter> parameters)
    {
        if (!TryGetOverload(symbol, arguments, parameters, out var overload))
        {
            throw new NotImplementedException(
                                              $"Aggregate function {symbol.Name}{SchemaDisplay.GetText(symbol)} is not implemented for argument types ({string.Join(", ", arguments.Select(arg => SchemaDisplay.GetText(arg.ResultType)))}).");
        }

        Debug.Assert(overload != null);
        return overload;
    }

    public static bool TryGetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters,
        out AggregateOverloadInfo? overload)
    {
        if (!aggregates.TryGetValue(symbol, out var aggregateInfo))
        {
            overload = null;
            return false;
        }

        overload = BuiltInsHelper.PickOverload(aggregateInfo.Overloads, arguments);
        return overload != null;
    }
}
