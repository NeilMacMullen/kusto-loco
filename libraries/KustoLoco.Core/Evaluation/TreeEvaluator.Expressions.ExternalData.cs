//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation;

// The engine's fallback externaldata resolver, created on first use so a query that never mentions
// externaldata pays nothing (and no HttpClient is constructed). Shared and thread-safe, matching the
// lifetime of a long-lived HttpClient.
internal static class DefaultExternalDataResolver
{
    private static readonly Lazy<IExternalDataResolver> Instance =
        new(() => new HttpExternalDataResolver(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static IExternalDataResolver Value => Instance.Value;
}

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitExternalDataExpression(IRExternalDataExpression node, EvaluationContext context)
    {
        var schema = (TableSymbol)node.ResultType;
        // A host-registered resolver wins; otherwise the engine's own HTTPS resolver runs, so externaldata
        // resolves out of the box as it does in ADX instead of requiring every host to write a fetcher. Its
        // defaults are deliberately conservative (HTTPS, public addresses only, bounded time and size) — see
        // HttpExternalDataResolver; register your own to tighten, widen or replace that policy.
        var resolver = context.Providers?.Get<IExternalDataResolver>() ?? DefaultExternalDataResolver.Value;

        // The resolver fetches and splits each URI; the engine types the cells per the declared schema.
        var cells = new List<IReadOnlyList<string>>();
        foreach (var uri in node.Uris)
            cells.AddRange(resolver.ResolveRows(uri, node.Format));

        var columns = new BaseColumn[schema.Columns.Count];
        for (var j = 0; j < schema.Columns.Count; j++)
        {
            var type = schema.Columns[j].Type;
            var data = new object?[cells.Count];
            for (var i = 0; i < cells.Count; i++)
                data[i] = j < cells[i].Count ? ConvertCell(cells[i][j], type) : null;
            columns[j] = ColumnHelpers.CreateFromObjectArray(data, type);
        }

        return TabularResult.CreateUnvisualized(new InMemoryTableSource(schema, columns));
    }

    /// <summary>Type one text cell per its declared column type. An unparseable cell is null rather than an error:
    /// a single malformed value must not fail the whole query, matching how a missing value behaves elsewhere.</summary>
    private static object? ConvertCell(string? text, TypeSymbol type)
    {
        if (text is null) return null;
        var trimmed = text.Trim();
        if (type == ScalarTypes.String) return text;
        if (trimmed.Length == 0) return null;

        if (type == ScalarTypes.Bool)
            return bool.TryParse(trimmed, out var b) ? b
                : long.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bi) ? bi != 0
                : null;
        if (type == ScalarTypes.Int)
            return int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : null;
        if (type == ScalarTypes.Long)
            return long.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l) ? l : null;
        if (type == ScalarTypes.Real)
            return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;
        if (type == ScalarTypes.Decimal)
            return decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out var m) ? m : null;
        if (type == ScalarTypes.DateTime)
            return DateTime.TryParse(trimmed, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt) ? dt : null;
        if (type == ScalarTypes.TimeSpan)
            return TimeSpan.TryParse(trimmed, CultureInfo.InvariantCulture, out var ts) ? ts : null;
        if (type == ScalarTypes.Guid)
            return Guid.TryParse(trimmed, out var g) ? g : null;
        if (type == ScalarTypes.Dynamic)
        {
            try { return JsonNode.Parse(trimmed); }
            catch (System.Text.Json.JsonException) { return JsonValue.Create(text); }
        }
        return text;
    }
}
