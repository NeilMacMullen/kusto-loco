//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Symbols;
using KustoLoco.Core.Evaluation.BuiltIns;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

internal class IRBuiltInScalarFunctionCallNode : IRExpressionNode
{
    public IRBuiltInScalarFunctionCallNode(Signature signature, ScalarOverloadInfo overloadInfo,
        IReadOnlyList<Parameter> parameters, IRListNode<IRExpressionNode> arguments, TypeSymbol resultType)
        : base(resultType, GetResultKind(arguments))
    {
        Signature = signature ?? throw new ArgumentNullException(nameof(signature));
        OverloadInfo = overloadInfo ?? throw new ArgumentNullException(nameof(overloadInfo));
        Parameters = parameters ?? new List<Parameter>();
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }

    public Signature Signature { get; }
    public ScalarOverloadInfo OverloadInfo { get; }
    public IReadOnlyList<Parameter> Parameters { get; }
    public IRListNode<IRExpressionNode> Arguments { get; }

    public override int ChildCount => 1;

    public override IRNode GetChild(int index) =>
        index switch
        {
            0 => Arguments,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        =>
            visitor.VisitBuiltInScalarFunctionCall(this, context);

    public override string ToString() =>
        $"BuiltInScalarFunctionCall('{Signature.Symbol.Name}' {SchemaDisplay.GetText(Signature.Symbol)}): returnType {SchemaDisplay.GetText(ResultType)}";
}
