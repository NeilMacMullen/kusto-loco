// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

internal class IRPrintOperatorNode : IRQueryOperatorNode
{
    public IRPrintOperatorNode(IRListNode<IRExpressionNode> expressions, TypeSymbol resultType)
        : base(resultType) =>
        Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));

    public IRListNode<IRExpressionNode> Expressions { get; }

    public override int ChildCount => 1;

    public override IRNode GetChild(int index) =>
        index switch
        {
            0 => Expressions,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };


    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
         =>
        visitor.VisitPrintOperator(this, context);

    public override string ToString() => $"PrintOperator: {SchemaDisplay.GetText(ResultType)}";
}