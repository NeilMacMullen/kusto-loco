// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Kusto.Language;
using Kusto.Language.Symbols;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

namespace KustoLoco.Core.Evaluation.BuiltIns;

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
                new ScalarOverloadInfo(new BetweenOperatorImplOfint(false), ScalarTypes.Bool,
                    ScalarTypes.Int, ScalarTypes.Int,
                    ScalarTypes.Int),
                new ScalarOverloadInfo(new IntBetweenOperatorImpl(false),
                    ScalarTypes.Bool,
                    ScalarTypes.Int, ScalarTypes.Long,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new BetweenOperatorImplOflong(false),
                    ScalarTypes.Bool,
                    ScalarTypes.Long, ScalarTypes.Long,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new BetweenOperatorImplOfdecimal(false),
                    ScalarTypes.Bool,
                    ScalarTypes.Decimal, ScalarTypes.Decimal,
                    ScalarTypes.Decimal),
                new ScalarOverloadInfo(new BetweenOperatorImplOfdouble(false),
                    ScalarTypes.Bool,
                    ScalarTypes.Real, ScalarTypes.Real,
                    ScalarTypes.Real),
                new ScalarOverloadInfo(new BetweenOperatorImplOfDateTime(false),
                    ScalarTypes.Bool,
                    ScalarTypes.DateTime, ScalarTypes.DateTime,
                    ScalarTypes.DateTime),
                new ScalarOverloadInfo(new BetweenOperatorImplOfTimeSpan(false),
                    ScalarTypes.Bool,
                    ScalarTypes.TimeSpan, ScalarTypes.TimeSpan,
                    ScalarTypes.TimeSpan),
                new ScalarOverloadInfo(new BetweenOperatorDateTimeWithTimespanImpl(false),
                    ScalarTypes.Bool,
                    ScalarTypes.DateTime, ScalarTypes.DateTime,
                    ScalarTypes.TimeSpan)
            )
        );

        operators.Add(Operators.NotBetween,
            new ScalarFunctionInfo(
                new ScalarOverloadInfo(new BetweenOperatorImplOfint(true), ScalarTypes.Bool,
                    ScalarTypes.Int, ScalarTypes.Long,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new BetweenOperatorImplOflong(true),
                    ScalarTypes.Bool,
                    ScalarTypes.Long, ScalarTypes.Long,
                    ScalarTypes.Long),
                new ScalarOverloadInfo(new BetweenOperatorImplOfint(true),
                    ScalarTypes.Bool,
                    ScalarTypes.Int, ScalarTypes.Int, ScalarTypes.Int),
                new ScalarOverloadInfo(new BetweenOperatorImplOfDateTime(true),
                    ScalarTypes.Bool,
                    ScalarTypes.DateTime, ScalarTypes.DateTime,
                    ScalarTypes.DateTime))
        );
        ContainsFunction.Register(operators);
        ContainsCsFunction.Register(operators);
        HasFunction.Register(operators);
        HasCsFunction.Register(operators);
        InOperator.Register(operators);
        InCsOperator.Register(operators);
        NotInOperator.Register(operators);
        NotInCsOperator.Register(operators);
        HasAnyOperator.Register(operators);
        HasAllOperator.Register(operators);

        NotContainsFunction.Register(operators);
        NotContainsCsFunction.Register(operators);
        NotHasFunction.Register(operators);
        NotHasCsFunction.Register(operators);

        StartsWithFunction.Register(operators);
        StartsWithCsFunction.Register(operators);
        NotStartsWithFunction.Register(operators);
        NotStartsWithCsFunction.Register(operators);

        EndsWithFunction.Register(operators);
        EndsWithCsFunction.Register(operators);
        NotEndsWithFunction.Register(operators);
        NotEndsWithCsFunction.Register(operators);

        MatchRegexFunction.Register(operators);
    }

    public static ScalarOverloadInfo GetOverload(OperatorSymbol symbol,
        TypeSymbol resultType,
        IRExpressionNode[] arguments)
    {
        if (!TryGetOverload(symbol,resultType, arguments, out var overload))
            throw new NotImplementedException(
                $"Operator {SchemaDisplay.GetText(symbol)} is not implemented for argument types ({string.Join(", ", arguments.Select(arg => SchemaDisplay.GetText(arg.ResultType)))}).");

        return overload!;
    }

    public static bool TryGetOverload(OperatorSymbol symbol,
        TypeSymbol resultType,
        IRExpressionNode[] arguments,
        out ScalarOverloadInfo? overload)
    {
        if (!operators.TryGetValue(symbol, out var operatorInfo))
        {
            overload = null;
            return false;
        }

        overload = BuiltInsHelper.PickOverload(resultType,operatorInfo.Overloads, arguments);
        return overload != null;
    }
}
