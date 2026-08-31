//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitExternalDataExpression(ExternalDataExpression node)
    {
        var uris = new List<string>();
        foreach (var uriElement in node.URIs)
            if (uriElement.Element.Accept(this) is IRLiteralExpressionNode { Value: string uri })
                uris.Add(uri);

        var format = "csv";
        var ignoreFirstRecord = false;
        if (node.WithClause != null)
            foreach (var property in node.WithClause.Properties)
            {
                if (property.Element is not NamedParameter np) continue;
                var name = np.Name.SimpleName;
                var value = np.Expression.Accept(this);

                if (string.Equals(name, "format", StringComparison.OrdinalIgnoreCase))
                {
                    if (value is IRLiteralExpressionNode { Value: string f }) format = f;
                }
                else if (string.Equals(name, "ignoreFirstRecord", StringComparison.OrdinalIgnoreCase))
                {
                    ignoreFirstRecord = value is IRLiteralExpressionNode { Value: var v } &&
                                        v switch
                                        {
                                            bool b => b,
                                            string s => bool.TryParse(s, out var parsed) && parsed,
                                            _ => false,
                                        };
                }
                else
                {
                    // Refuse rather than ignore: silently dropping a property the author wrote means the query
                    // returns a DIFFERENT result than they asked for, with nothing to indicate it.
                    throw new InvalidOperationException(
                        $"externaldata does not support the '{name}' property (supported: format, ignoreFirstRecord).");
                }
            }

        return new IRExternalDataExpression(uris, format, ignoreFirstRecord, node.ResultType);
    }
}
