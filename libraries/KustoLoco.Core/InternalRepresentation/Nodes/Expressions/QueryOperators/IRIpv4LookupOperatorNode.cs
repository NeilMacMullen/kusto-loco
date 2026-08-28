//
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

// evaluate ipv4_lookup(LookupTable, SourceIpKey, IpLookupKey): joins the source rows to LookupTable by matching the
// source IP (SourceIpKey) against the CIDR range in LookupTable's IpLookupKey column, appending the lookup columns.
internal class IRIpv4LookupOperatorNode : IRQueryOperatorNode
{
    public IRIpv4LookupOperatorNode(IRExpressionNode lookupTable, IRExpressionNode sourceIp, string lookupIpColumn,
        bool returnUnmatched, TypeSymbol resultType)
        : base(resultType)
    {
        LookupTable = lookupTable ?? throw new ArgumentNullException(nameof(lookupTable));
        SourceIp = sourceIp ?? throw new ArgumentNullException(nameof(sourceIp));
        LookupIpColumn = lookupIpColumn;
        ReturnUnmatched = returnUnmatched;
    }

    public IRExpressionNode LookupTable { get; }
    public IRExpressionNode SourceIp { get; }
    public string LookupIpColumn { get; }
    public bool ReturnUnmatched { get; }

    public override int ChildCount => 2;

    public override IRNode GetChild(int index) => index == 0 ? LookupTable : SourceIp;

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        => visitor.VisitIpv4LookupOperator(this, context);
}
