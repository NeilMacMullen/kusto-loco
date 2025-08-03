﻿//
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

internal class IRProjectOperatorNode : IRQueryOperatorNode
{
    public readonly bool RequiresSerializatiom;

    public IRProjectOperatorNode(IRListNode<IROutputColumnNode> columns, TypeSymbol resultType,bool requiresSerializatiom)
        : base(resultType)
    {
        RequiresSerializatiom = requiresSerializatiom;
        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
    }

    public IRListNode<IROutputColumnNode> Columns { get; }

    public override int ChildCount => 1;

    public override IRNode GetChild(int index) =>
        index switch
        {
            0 => Columns,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };


    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
         =>
        visitor.VisitProjectOperator(this, context);

    public override string ToString() => $"ProjectOperator: {SchemaDisplay.GetText(ResultType)}";
}

