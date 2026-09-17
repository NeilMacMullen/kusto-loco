//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

// evaluate ipv4_lookup / ipv6_lookup (LookupTable, SourceIpKey, IpLookupKey [, ExtraKey1 .. ExtraKeyN]
// [, return_unmatched]): joins the source rows to LookupTable by matching the source IP (SourceIpKey) against the
// CIDR range in LookupTable's IpLookupKey column, appending the lookup columns. ExtraKeys are columns present in
// BOTH tables that must additionally match by equality; return_unmatched keeps unmatched source rows (lookup
// columns null) instead of dropping them.
//
// ONE node serves both plugins because they differ only in the address family used for the range test — every other
// part (tabular lookup, extra-key narrowing, unmatched handling, result shaping) is identical. Carrying a separate
// IPv6 node would duplicate the whole evaluator for one predicate.
internal class IRIpLookupOperatorNode : IRQueryOperatorNode
{
    public IRIpLookupOperatorNode(IRExpressionNode lookupTable, IRExpressionNode sourceIp, string lookupIpColumn,
        IReadOnlyList<string> extraKeys, bool returnUnmatched, bool isIpv6, TypeSymbol resultType)
        : base(resultType)
    {
        LookupTable = lookupTable ?? throw new ArgumentNullException(nameof(lookupTable));
        SourceIp = sourceIp ?? throw new ArgumentNullException(nameof(sourceIp));
        LookupIpColumn = lookupIpColumn;
        ExtraKeys = extraKeys ?? throw new ArgumentNullException(nameof(extraKeys));
        ReturnUnmatched = returnUnmatched;
        IsIpv6 = isIpv6;
    }

    public IRExpressionNode LookupTable { get; }
    public IRExpressionNode SourceIp { get; }
    public string LookupIpColumn { get; }
    public IReadOnlyList<string> ExtraKeys { get; }
    public bool ReturnUnmatched { get; }

    /// True for `ipv6_lookup`. The IPv6 matcher accepts both families (ADX maps IPv4 into IPv6-mapped space), so
    /// this selects the range predicate rather than restricting what the data may contain.
    public bool IsIpv6 { get; }

    public override int ChildCount => 2;

    public override IRNode GetChild(int index) => index == 0 ? LookupTable : SourceIp;

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        => visitor.VisitIpLookupOperator(this, context);
}
