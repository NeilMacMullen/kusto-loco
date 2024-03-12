// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using KustoLoco.Core.InternalRepresentation.Nodes.Statements;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Other;

internal class IRQueryBlockNode : IRNode
{
    public IRQueryBlockNode(IRListNode<IRStatementNode> statements) =>
        Statements = statements ?? throw new ArgumentNullException(nameof(statements));

    public IRListNode<IRStatementNode> Statements { get; }

    public override int ChildCount => 1;

    public override IRNode GetChild(int index) =>
        index switch
        {
            0 => Statements,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
         =>
        visitor.VisitQueryBlock(this, context);

    public override string ToString() => "QueryBlock";
}