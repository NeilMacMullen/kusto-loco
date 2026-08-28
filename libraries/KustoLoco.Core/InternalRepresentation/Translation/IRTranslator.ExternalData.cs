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
        if (node.WithClause != null)
            foreach (var property in node.WithClause.Properties)
                if (property.Element is NamedParameter np &&
                    string.Equals(np.Name.SimpleName, "format", StringComparison.OrdinalIgnoreCase) &&
                    np.Expression.Accept(this) is IRLiteralExpressionNode { Value: string f })
                    format = f;

        return new IRExternalDataExpression(uris, format, node.ResultType);
    }
}
