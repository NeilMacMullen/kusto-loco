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
    internal static class BuiltInAggregates
    {
        private static Dictionary<FunctionSymbol, AggregateInfo> aggregates = new();

        static BuiltInAggregates()
        {
            aggregates.Add(Aggregates.Count, new AggregateInfo(new AggregateOverloadInfo(new CountFunctionImpl(), ScalarTypes.Long)));
            
            aggregates.Add(Aggregates.CountIf, new AggregateInfo(new AggregateOverloadInfo(new CountIfFunctionImpl(), ScalarTypes.Long, ScalarTypes.Bool)));

            aggregates.Add(
                Aggregates.SumIf,
                new AggregateInfo(
                    new AggregateOverloadInfo(new SumIfIntFunctionImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Bool),
                    new AggregateOverloadInfo(new SumIfLongFunctionImpl(), ScalarTypes.Long, ScalarTypes.Long, ScalarTypes.Bool),
                    new AggregateOverloadInfo(new SumIfDoubleFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Bool)));

            aggregates.Add(
                Aggregates.Avg,
                new AggregateInfo(
                    new AggregateOverloadInfo(new AvgAggregateIntImpl(), ScalarTypes.Real, ScalarTypes.Int),
                    new AggregateOverloadInfo(new AvgAggregateLongImpl(), ScalarTypes.Real, ScalarTypes.Long),
                    new AggregateOverloadInfo(new AvgAggregateDoubleImpl(), ScalarTypes.Real, ScalarTypes.Real)));

            aggregates.Add(
                Aggregates.Sum,
                new AggregateInfo(
                    new AggregateOverloadInfo(new SumAggregateIntImpl(), ScalarTypes.Int, ScalarTypes.Int),
                    new AggregateOverloadInfo(new SumAggregateLongImpl(), ScalarTypes.Long, ScalarTypes.Long),
                    new AggregateOverloadInfo(new SumAggregateDoubleImpl(), ScalarTypes.Real, ScalarTypes.Real)));

            aggregates.Add(
                Aggregates.Min,
                new AggregateInfo(
                    new AggregateOverloadInfo(new MinAggregateIntImpl(), ScalarTypes.Int, ScalarTypes.Int),
                    new AggregateOverloadInfo(new MinAggregateLongImpl(), ScalarTypes.Long, ScalarTypes.Long),
                    new AggregateOverloadInfo(new MinAggregateDoubleImpl(), ScalarTypes.Real, ScalarTypes.Real),
                    new AggregateOverloadInfo(new MinAggregateDateTimeImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime),
                    new AggregateOverloadInfo(new MinAggregateTimeSpanImpl(), ScalarTypes.TimeSpan, ScalarTypes.TimeSpan)));

            aggregates.Add(
                Aggregates.Max,
                new AggregateInfo(
                    new AggregateOverloadInfo(new MaxAggregateIntImpl(), ScalarTypes.Int, ScalarTypes.Int),
                    new AggregateOverloadInfo(new MaxAggregateLongImpl(), ScalarTypes.Long, ScalarTypes.Long),
                    new AggregateOverloadInfo(new MaxAggregateDoubleImpl(), ScalarTypes.Real, ScalarTypes.Real),
                    new AggregateOverloadInfo(new MaxAggregateDateTimeImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime),
                    new AggregateOverloadInfo(new MaxAggregateTimeSpanImpl(), ScalarTypes.TimeSpan, ScalarTypes.TimeSpan)));

            aggregates.Add(
                Aggregates.Any,
                new AggregateInfo(
                    new AggregateOverloadInfo(new AnyFunctionImpl(), ScalarTypes.Bool, ScalarTypes.Bool),
                    new AggregateOverloadInfo(new AnyFunctionImpl(), ScalarTypes.Int, ScalarTypes.Int),
                    new AggregateOverloadInfo(new AnyFunctionImpl(), ScalarTypes.Long, ScalarTypes.Long),
                    new AggregateOverloadInfo(new AnyFunctionImpl(), ScalarTypes.Real, ScalarTypes.Real),
                    new AggregateOverloadInfo(new AnyFunctionImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime),
                    new AggregateOverloadInfo(new AnyFunctionImpl(), ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
                    new AggregateOverloadInfo(new AnyFunctionImpl(), ScalarTypes.String, ScalarTypes.String)));
        }

        public static AggregateOverloadInfo GetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters)
        {
            if (!TryGetOverload(symbol, arguments, parameters, out var overload))
            {
                throw new NotImplementedException($"Aggregate function {symbol.Display} is not implemented for argument types ({string.Join(", ", arguments.Select(arg => arg.ResultType.Display))}).");
            }

            Debug.Assert(overload != null);
            return overload;
        }

        public static bool TryGetOverload(FunctionSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters, out AggregateOverloadInfo? overload)
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
}
