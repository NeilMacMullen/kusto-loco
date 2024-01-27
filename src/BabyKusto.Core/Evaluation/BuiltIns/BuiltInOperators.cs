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

internal static class BuiltInOperators
{
    private static readonly Dictionary<OperatorSymbol, ScalarFunctionInfo> operators = new();

    static BuiltInOperators()
    {
        UnaryMinusFunction.Register(operators);

        AddFunction.Register(operators);
        SubtractFunction.Register(operators);


        MultiplyFunction.Register(operators);
        DivideFunction.Register(operators);
        ModuloFunction.Register(operators);

        GreaterThanFunction.Register(operators);
        GreaterThanOrEqualFunction.Register(operators);
        LessThanFunction.Register(operators);
        LessThanOrEqualFunction.Register(operators);

        operators.Add(
            Operators.Equal,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new EqualIntOperatorImpl(), ScalarTypes.Bool,
                    ScalarTypes.Int, ScalarTypes.Int),
                new ScalarOverloadInfo(new EqualLongOperatorImpl(), ScalarTypes.Bool,
                    ScalarTypes.Long,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new EqualDoubleOperatorImpl(), ScalarTypes.Bool,
                    ScalarTypes.Real,
                    ScalarTypes.Real),
                new ScalarOverloadInfo(new EqualStringOperatorImpl(), ScalarTypes.Bool,
                    ScalarTypes.String,
                    ScalarTypes.String),
                new ScalarOverloadInfo(new EqualTimeSpanOperatorImpl(), ScalarTypes.Bool,
                    ScalarTypes.TimeSpan,
                    ScalarTypes.TimeSpan),
                new ScalarOverloadInfo(new EqualDateTimeOperatorImpl(), ScalarTypes.Bool,
                    ScalarTypes.DateTime,
                    ScalarTypes.DateTime)));

        operators.Add(
            Operators.NotEqual,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new NotEqualIntOperatorImpl(), ScalarTypes.Bool,
                    ScalarTypes.Int,
                    ScalarTypes.Int),
                new ScalarOverloadInfo(new NotEqualLongOperatorImpl(), ScalarTypes.Bool,
                    ScalarTypes.Long,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new NotEqualDoubleOperatorImpl(), ScalarTypes.Bool,
                    ScalarTypes.Real,
                    ScalarTypes.Real),
                new ScalarOverloadInfo(new NotEqualStringOperatorImpl(), ScalarTypes.Bool,
                    ScalarTypes.String,
                    ScalarTypes.String),
                new ScalarOverloadInfo(new NotEqualTimeSpanOperatorImpl(),
                    ScalarTypes.Bool, ScalarTypes.TimeSpan,
                    ScalarTypes.TimeSpan),
                new ScalarOverloadInfo(new NotEqualDateTimeOperatorImpl(),
                    ScalarTypes.Bool, ScalarTypes.DateTime,
                    ScalarTypes.DateTime)));

        operators.Add(
            Operators.And,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new LogicalAndOperatorImpl(), ScalarTypes.Bool,
                    ScalarTypes.Bool,
                    ScalarTypes.Bool)));

        operators.Add(
            Operators.Or,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new LogicalOrOperatorImpl(), ScalarTypes.Bool,
                    ScalarTypes.Bool,
                    ScalarTypes.Bool)));

        //the "extra" parameter here is because the between couple is expanded to two
        //separate columns
        operators.Add(Operators.Between,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new IntBetweenOperatorImpl(false), ScalarTypes.Bool,
                    ScalarTypes.Int, ScalarTypes.Long,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new BetweenOperatorImpl<long>(false),
                    ScalarTypes.Bool,
                    ScalarTypes.Long, ScalarTypes.Long,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new BetweenOperatorImpl<DateTime>(false),
                    ScalarTypes.Bool,
                    ScalarTypes.DateTime, ScalarTypes.DateTime,
                    ScalarTypes.DateTime),
                new ScalarOverloadInfo(new BetweenOperatorDateTimeWithTimespanImpl(false),
                    ScalarTypes.Bool,
                    ScalarTypes.DateTime, ScalarTypes.DateTime,
                    ScalarTypes.TimeSpan)
            )
        );

        operators.Add(Operators.NotBetween,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new IntBetweenOperatorImpl(true), ScalarTypes.Bool,
                    ScalarTypes.Int, ScalarTypes.Long,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new BetweenOperatorImpl<long>(true),
                    ScalarTypes.Bool,
                    ScalarTypes.Long, ScalarTypes.Long,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new BetweenOperatorImpl<int>(true),
                    ScalarTypes.Bool,
                    ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
                new ScalarOverloadInfo(new BetweenOperatorImpl<DateTime>(true),
                    ScalarTypes.Bool,
                    ScalarTypes.DateTime, ScalarTypes.DateTime,
                    ScalarTypes.DateTime))
        );

        operators.Add(Operators.Contains,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new ContainsOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));
        operators.Add(Operators.ContainsCs,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new ContainsCsOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));
        operators.Add(Operators.NotContains,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new NotContainsOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));
        operators.Add(Operators.NotContainsCs,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new NotContainsCsOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));
        operators.Add(Operators.StartsWith,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new StartsWithOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));
        operators.Add(Operators.StartsWithCs,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new StartsWithCsOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));
        operators.Add(Operators.NotStartsWith,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new NotStartsWithOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));
        operators.Add(Operators.NotStartsWithCs,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new NotStartsWithCsOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));
        operators.Add(Operators.EndsWith,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new EndsWithOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));
        operators.Add(Operators.EndsWithCs,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new EndsWithCsOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));
        operators.Add(Operators.NotEndsWith,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new NotEndsWithOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));
        operators.Add(Operators.NotEndsWithCs,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new NotEndsWithCsOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));

        operators.Add(Operators.MatchRegex,
            new ScalarFunctionInfo(new ScalarOverloadInfo(new MatchRegexOperatorImpl(), ScalarTypes.Bool,
                ScalarTypes.String, ScalarTypes.String)));
    }

    public static ScalarOverloadInfo GetOverload(OperatorSymbol symbol, IRExpressionNode[] arguments)
    {
        if (!TryGetOverload(symbol, arguments, out var overload))
        {
            throw new NotImplementedException(
                $"Operator {symbol.Name}{SchemaDisplay.GetText(symbol)} is not implemented for argument types ({string.Join(", ", arguments.Select(arg => SchemaDisplay.GetText(arg.ResultType)))}).");
        }

        return overload!;
    }

    public static bool TryGetOverload(OperatorSymbol symbol, IRExpressionNode[] arguments,
        out ScalarOverloadInfo? overload)
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