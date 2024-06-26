﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

internal class IRMaterializeExpressionNode : IRExpressionNode
{
    public IRMaterializeExpressionNode(IRExpressionNode expression, TypeSymbol resultType)
        : base(resultType, expression.ResultKind) =>
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));

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
        visitor.VisitMaterializeExpression(this, context);

    public override string ToString() => "MaterializeExpression";
}