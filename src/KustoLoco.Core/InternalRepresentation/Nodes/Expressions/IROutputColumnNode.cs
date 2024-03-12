// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation;

internal class IROutputColumnNode : IRExpressionNode
{
    public IROutputColumnNode(ColumnSymbol symbol, IRExpressionNode expression)
        : base(symbol.Type, expression.ResultKind)
    {
        Symbol = symbol;
        Expression = expression;
    }

    public ColumnSymbol Symbol { get; }
    public IRExpressionNode Expression { get; }

    public override int ChildCount => 1;

    public override IRNode GetChild(int index) =>
        index switch
        {
            0 => Expression,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
         =>
        visitor.VisitOutputColumn(this, context);

    public override string ToString() => $"OutputColumn({Symbol.Name}: {SchemaDisplay.GetText(ResultType)})";
}