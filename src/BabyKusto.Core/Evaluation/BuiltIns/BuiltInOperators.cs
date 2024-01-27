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

        EqualFunction.Register(operators);
        EqualTildeFunction.Register(operators);

        NotEqualFunction.Register(operators);

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
        ContainsFunction.Register(operators);
        ContainsCsFunction.Register(operators);

        NotContainsFunction.Register(operators);
        NotContainsCsFunction.Register(operators);

        StartsWithFunction.Register(operators);
        StartsWithCsFunction.Register(operators);
        NotStartsWithFunction.Register(operators);
        NotStartsWithCsFunction.Register(operators);

        EndsWithFunction.Register(operators);
        EndsWithCsFunction.Register(operators);
        NotEndsWithFunction.Register(operators);
        NotEndsWithCsFunction.Register(operators);


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