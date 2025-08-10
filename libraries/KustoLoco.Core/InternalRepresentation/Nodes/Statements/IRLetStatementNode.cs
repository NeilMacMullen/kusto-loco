﻿//
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Statements;

internal class IRLetStatementNode : IRStatementNode
{
    public IRLetStatementNode(Symbol symbol, IRExpressionNode expression)
    {
        Symbol = symbol;
        Expression = expression;
    }

    public Symbol Symbol { get; }
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
            visitor.VisitLetStatement(this, context);

    public override string ToString() => $"LetStatement {{{Symbol.Name} = ...}}";
}