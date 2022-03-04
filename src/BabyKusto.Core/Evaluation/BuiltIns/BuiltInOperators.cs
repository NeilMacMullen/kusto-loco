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
    internal static class BuiltInOperators
    {
        private static Dictionary<OperatorSymbol, ScalarFunctionInfo> operators = new();

        static BuiltInOperators()
        {
            operators.Add(
                Operators.UnaryMinus,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new UnaryMinusIntOperatorImpl(), ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new UnaryMinusLongOperatorImpl(), ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new UnaryMinusDoubleOperatorImpl(), ScalarTypes.Real, ScalarTypes.Real),
                    new ScalarOverloadInfo(new UnaryMinusTimeSpanOperatorImpl(), ScalarTypes.TimeSpan, ScalarTypes.TimeSpan)));

            operators.Add(
                Operators.Add,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new AddIntOperatorImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new AddLongOperatorImpl(), ScalarTypes.Long, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new AddDoubleOperatorImpl(), ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real),
                    new ScalarOverloadInfo(new AddTimeSpanOperatorImpl(), ScalarTypes.TimeSpan, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
                    new ScalarOverloadInfo(new AddDateTimeTimeSpanOperatorImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime, ScalarTypes.TimeSpan)));

            operators.Add(
                Operators.Subtract,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new SubtractIntOperatorImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new SubtractLongOperatorImpl(), ScalarTypes.Long, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new SubtractDoubleOperatorImpl(), ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real),
                    new ScalarOverloadInfo(new SubtractTimeSpanOperatorImpl(), ScalarTypes.TimeSpan, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
                    new ScalarOverloadInfo(new SubtractDateTimeTimeSpanOperatorImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime, ScalarTypes.TimeSpan)));

            operators.Add(
                Operators.Multiply,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new MultiplyIntOperatorImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new MultiplyLongOperatorImpl(), ScalarTypes.Long, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new MultiplyDoubleOperatorImpl(), ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real),
                    new ScalarOverloadInfo(new MultiplyLongTimeSpanOperatorImpl(), ScalarTypes.TimeSpan, ScalarTypes.Long, ScalarTypes.TimeSpan)));

            operators.Add(
                Operators.Divide,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new DivideIntOperatorImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new DivideLongOperatorImpl(), ScalarTypes.Long, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new DivideDoubleOperatorImpl(), ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real),
                    new ScalarOverloadInfo(new DivideTimeSpanOperatorImpl(), ScalarTypes.Real, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan)));

            operators.Add(
                Operators.Modulo,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new ModuloIntOperatorImpl(), ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new ModuloLongOperatorImpl(), ScalarTypes.Long, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new ModuloDoubleOperatorImpl(), ScalarTypes.Real, ScalarTypes.Real, ScalarTypes.Real),
                    new ScalarOverloadInfo(new ModuloDateTimeTimeSpanOperatorImpl(), ScalarTypes.TimeSpan, ScalarTypes.DateTime, ScalarTypes.TimeSpan)));

            operators.Add(
                Operators.GreaterThan,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new GreaterThanIntOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new GreaterThanLongOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new GreaterThanDoubleOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Real, ScalarTypes.Real),
                    new ScalarOverloadInfo(new GreaterThanTimeSpanOperatorImpl(), ScalarTypes.Bool, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
                    new ScalarOverloadInfo(new GreaterThanDateTimeOperatorImpl(), ScalarTypes.Bool, ScalarTypes.DateTime, ScalarTypes.DateTime)));

            operators.Add(
                Operators.GreaterThanOrEqual,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new GreaterThanOrEqualIntOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new GreaterThanOrEqualLongOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new GreaterThanOrEqualDoubleOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Real, ScalarTypes.Real),
                    new ScalarOverloadInfo(new GreaterThanOrEqualTimeSpanOperatorImpl(), ScalarTypes.Bool, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
                    new ScalarOverloadInfo(new GreaterThanOrEqualDateTimeOperatorImpl(), ScalarTypes.Bool, ScalarTypes.DateTime, ScalarTypes.DateTime)));

            operators.Add(
                Operators.LessThan,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new LessThanIntOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new LessThanLongOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new LessThanDoubleOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Real, ScalarTypes.Real),
                    new ScalarOverloadInfo(new LessThanTimeSpanOperatorImpl(), ScalarTypes.Bool, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
                    new ScalarOverloadInfo(new LessThanDateTimeOperatorImpl(), ScalarTypes.Bool, ScalarTypes.DateTime, ScalarTypes.DateTime)));

            operators.Add(
                Operators.LessThanOrEqual,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new LessThanOrEqualIntOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new LessThanOrEqualLongOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new LessThanOrEqualDoubleOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Real, ScalarTypes.Real),
                    new ScalarOverloadInfo(new LessThanOrEqualTimeSpanOperatorImpl(), ScalarTypes.Bool, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
                    new ScalarOverloadInfo(new LessThanOrEqualDateTimeOperatorImpl(), ScalarTypes.Bool, ScalarTypes.DateTime, ScalarTypes.DateTime)));

            operators.Add(
                Operators.Equal,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new EqualIntOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new EqualLongOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new EqualDoubleOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Real, ScalarTypes.Real),
                    new ScalarOverloadInfo(new EqualStringOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String),
                    new ScalarOverloadInfo(new EqualTimeSpanOperatorImpl(), ScalarTypes.Bool, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
                    new ScalarOverloadInfo(new EqualDateTimeOperatorImpl(), ScalarTypes.Bool, ScalarTypes.DateTime, ScalarTypes.DateTime)));

            operators.Add(
                Operators.NotEqual,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new NotEqualIntOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Int, ScalarTypes.Int),
                    new ScalarOverloadInfo(new NotEqualLongOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Long, ScalarTypes.Long),
                    new ScalarOverloadInfo(new NotEqualDoubleOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Real, ScalarTypes.Real),
                    new ScalarOverloadInfo(new NotEqualStringOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String),
                    new ScalarOverloadInfo(new NotEqualTimeSpanOperatorImpl(), ScalarTypes.Bool, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),
                    new ScalarOverloadInfo(new NotEqualDateTimeOperatorImpl(), ScalarTypes.Bool, ScalarTypes.DateTime, ScalarTypes.DateTime)));

            operators.Add(
                Operators.And,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new LogicalAndOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Bool, ScalarTypes.Bool)));

            operators.Add(
                Operators.Or,
                new ScalarFunctionInfo(
                    new ScalarOverloadInfo(new LogicalOrOperatorImpl(), ScalarTypes.Bool, ScalarTypes.Bool, ScalarTypes.Bool)));

            operators.Add(Operators.Contains, new ScalarFunctionInfo(new ScalarOverloadInfo(new ContainsOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String)));
            operators.Add(Operators.ContainsCs, new ScalarFunctionInfo(new ScalarOverloadInfo(new ContainsCsOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String)));
            operators.Add(Operators.NotContains, new ScalarFunctionInfo(new ScalarOverloadInfo(new NotContainsOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String)));
            operators.Add(Operators.NotContainsCs, new ScalarFunctionInfo(new ScalarOverloadInfo(new NotContainsCsOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String)));
            operators.Add(Operators.StartsWith, new ScalarFunctionInfo(new ScalarOverloadInfo(new StartsWithOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String)));
            operators.Add(Operators.StartsWithCs, new ScalarFunctionInfo(new ScalarOverloadInfo(new StartsWithCsOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String)));
            operators.Add(Operators.NotStartsWith, new ScalarFunctionInfo(new ScalarOverloadInfo(new NotStartsWithOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String)));
            operators.Add(Operators.NotStartsWithCs, new ScalarFunctionInfo(new ScalarOverloadInfo(new NotStartsWithCsOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String)));
            operators.Add(Operators.EndsWith, new ScalarFunctionInfo(new ScalarOverloadInfo(new EndsWithOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String)));
            operators.Add(Operators.EndsWithCs, new ScalarFunctionInfo(new ScalarOverloadInfo(new EndsWithCsOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String)));
            operators.Add(Operators.NotEndsWith, new ScalarFunctionInfo(new ScalarOverloadInfo(new NotEndsWithOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String)));
            operators.Add(Operators.NotEndsWithCs, new ScalarFunctionInfo(new ScalarOverloadInfo(new NotEndsWithCsOperatorImpl(), ScalarTypes.Bool, ScalarTypes.String, ScalarTypes.String)));
        }

        public static ScalarOverloadInfo GetOverload(OperatorSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters)
        {
            if (!TryGetOverload(symbol, arguments, parameters, out var overload))
            {
                throw new NotImplementedException($"Operator {symbol.Display} is not implemented for argument types ({string.Join(", ", arguments.Select(arg => arg.ResultType.Display))}).");
            }

            return overload!;
        }
        public static bool TryGetOverload(OperatorSymbol symbol, IRExpressionNode[] arguments, List<Parameter> parameters, out ScalarOverloadInfo? overload)
        {
            if (!operators.TryGetValue(symbol, out var operatorInfo))
            {
                overload = null;
                return false;
            }

            overload = BuiltInsHelper.PickOverload(operatorInfo.Overloads, arguments);
            return overload != null;
        }
    }
}
