//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitEvaluateOperator(EvaluateOperator node)
    {
        var pluginName = node.FunctionCall.Name.SimpleName;
        if (!string.Equals(pluginName, "ipv4_lookup", StringComparison.OrdinalIgnoreCase))
            throw new NotImplementedException($"evaluate plugin '{pluginName}' is not supported.");

        // ipv4_lookup(LookupTable, SourceIPv4Key, IPv4LookupKey [, ExtraKey1 .. ExtraKeyN] [, return_unmatched])
        var args = node.FunctionCall.ArgumentList.Expressions;
        if (args.Count < 3)
            throw new NotImplementedException("ipv4_lookup requires (LookupTable, SourceIpKey, IpLookupKey).");

        // LookupTable resolves as a tabular reference; SourceIpKey is a source-row expression; IpLookupKey is the name
        // of the CIDR column in the lookup table, taken as a bareword (it is not a source column).
        var lookupTable = (IRExpressionNode)args[0].Element.Accept(this);
        var sourceIp = (IRExpressionNode)args[1].Element.Accept(this);
        var lookupIpColumn = NameOf(args[2].Element);

        // The optional trailing return_unmatched flag may be written positionally (`.., true`) or named
        // (`.., return_unmatched = true`). Any other trailing arguments are ExtraKeys: column names that must exist in
        // BOTH tables and match by equality, narrowing the IPv4 match (like additional join keys).
        var returnUnmatched = false;
        var extraKeys = new List<string>();
        for (var i = 3; i < args.Count; i++)
        {
            var element = args[i].Element;
            if (TryGetBooleanFlag(element, out var flag))
            {
                returnUnmatched = flag;
                continue;
            }
            extraKeys.Add(NameOf(element));
        }

        return new IRIpv4LookupOperatorNode(lookupTable, sourceIp, lookupIpColumn, extraKeys, returnUnmatched,
            node.ResultType);
    }

    private static string NameOf(Expression expression) =>
        expression is NameReference nr ? nr.SimpleName : expression.ToString().Trim();

    // Recognises `true` / `false`, and the named form `return_unmatched = true` (a named argument parses as a simple
    // named expression whose value is the literal).
    private static bool TryGetBooleanFlag(Expression expression, out bool value)
    {
        value = false;
        var candidate = expression is SimpleNamedExpression named ? named.Expression : expression;
        if (candidate is LiteralExpression literal && literal.LiteralValue is bool b)
        {
            value = b;
            return true;
        }
        return false;
    }
}
