﻿//
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;
using KustoLoco.Core.Evaluation.BuiltIns;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

internal class IRUnaryExpressionNode : IRExpressionNode
{
    public IRUnaryExpressionNode(Signature signature, ScalarOverloadInfo overloadInfo, IRExpressionNode expression,
        TypeSymbol resultType)
        : base(resultType, expression.ResultKind)
    {
        Signature = signature ?? throw new ArgumentNullException(nameof(signature));
        OverloadInfo = overloadInfo ?? throw new ArgumentNullException(nameof(overloadInfo));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public Signature Signature { get; }
    public ScalarOverloadInfo OverloadInfo { get; }
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
        visitor.VisitUnaryExpression(this, context);

    public override string ToString() =>
        $"UnaryExpression({SchemaDisplay.GetText(Signature.Symbol)}): {SchemaDisplay.GetText(ResultType)}";
}