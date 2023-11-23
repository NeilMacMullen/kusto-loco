// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation;

internal enum IRJoinKind
{
    InnerUnique,
    Inner,
    LeftOuter,
    RightOuter,
    FullOuter,
    LeftSemi,
    RightSemi,
    LeftAnti,
    RightAnti,
}

internal class IRJoinOperatorNode : IRQueryOperatorNode
{
    public IRJoinOperatorNode(IRJoinKind kind, IRExpressionNode expression, List<IRJoinOnClause> onClauses,
        TypeSymbol resultType)
        : base(resultType)
    {
        Kind = kind;
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        OnClauses = onClauses ?? throw new ArgumentNullException(nameof(onClauses));
    }

    public IRJoinKind Kind { get; }
    public IRExpressionNode Expression { get; }
    public List<IRJoinOnClause> OnClauses { get; }

    public override int ChildCount => 1;

    public override IRNode GetChild(int index) =>
        index switch
        {
            0 => Expression,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
         =>
        visitor.VisitJoinOperator(this, context);

    public override string ToString() =>
        $"JoinOperator(kind={Kind}, {string.Join(", ", OnClauses)}): {SchemaDisplay.GetText(ResultType)}";
}