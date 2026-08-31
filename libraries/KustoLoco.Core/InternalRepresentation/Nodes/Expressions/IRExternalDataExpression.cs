//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

// externaldata (Col:type, ...) [uri, ...] with(format=...): a tabular expression whose rows come from data the HOST
// fetches. The engine holds only the declared schema, the URIs and the requested format; fetching is delegated to a
// host-registered IExternalDataResolver so the engine itself never performs network access.
internal class IRExternalDataExpression : IRExpressionNode
{
    public IRExternalDataExpression(IReadOnlyList<string> uris, string format, TypeSymbol resultType)
        : base(resultType, EvaluatedExpressionKind.Table)
    {
        Uris = uris ?? throw new ArgumentNullException(nameof(uris));
        Format = format;
    }

    public IReadOnlyList<string> Uris { get; }

    public string Format { get; }

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        => visitor.VisitExternalDataExpression(this, context);

    public override string ToString() => $"ExternalDataExpression: {SchemaDisplay.GetText(ResultType)}";
}
