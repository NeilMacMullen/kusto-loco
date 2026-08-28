//
// Licensed under the MIT License.

using System;
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

        var args = node.FunctionCall.ArgumentList.Expressions;
        if (args.Count < 3)
            throw new NotImplementedException("ipv4_lookup requires (LookupTable, SourceIpKey, IpLookupKey).");

        // LookupTable resolves as a tabular reference; SourceIpKey is a source-row expression; IpLookupKey is the name
        // of the CIDR column in the lookup table, taken as a bareword (it is not a source column).
        var lookupTable = (IRExpressionNode)args[0].Element.Accept(this);
        var sourceIp = (IRExpressionNode)args[1].Element.Accept(this);
        var lookupIpColumn = args[2].Element is NameReference nr ? nr.SimpleName : args[2].Element.ToString().Trim();

        return new IRIpv4LookupOperatorNode(lookupTable, sourceIp, lookupIpColumn, false, node.ResultType);
    }
}
