//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

// externaldata (Col:type, ...) [uri, ...] with(format=..., ignoreFirstRecord=...): a tabular expression whose rows
// come from data fetched at evaluation time. The engine holds only the declared schema, the URIs and the requested
// properties; fetching is delegated to an IExternalDataResolver (the built-in HTTPS one, or the host's).
internal class IRExternalDataExpression : IRExpressionNode
{
    public IRExternalDataExpression(IReadOnlyList<string> uris, string format, bool ignoreFirstRecord, TypeSymbol resultType)
        : base(resultType, EvaluatedExpressionKind.Table)
    {
        Uris = uris ?? throw new ArgumentNullException(nameof(uris));
        Format = format;
        IgnoreFirstRecord = ignoreFirstRecord;
    }

    public IReadOnlyList<string> Uris { get; }

    public string Format { get; }

    /// <summary>ADX's <c>ignoreFirstRecord</c>: drop each URI's first record, which is how a file carrying a header
    /// row is read (the declared schema names the columns, so the header itself is not data).</summary>
    public bool IgnoreFirstRecord { get; }

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        => visitor.VisitExternalDataExpression(this, context);

    public override string ToString() => $"ExternalDataExpression: {SchemaDisplay.GetText(ResultType)}";
}
