//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitExternalDataExpression(IRExternalDataExpression node, EvaluationContext context)
    {
        var schema = (TableSymbol)node.ResultType;
        var resolver = context.Providers?.Get<IExternalDataResolver>()
            // The engine performs no I/O of its own, so there is nothing sane to default to: fail LOUD rather than
            // yield an empty table, which would silently turn an unreachable list into a no-match.
            ?? throw new InvalidOperationException(
                "externaldata requires a host-provided IExternalDataResolver (register one with " +
                "KustoQueryContext.SetExternalDataResolver); the engine performs no network or file access itself.");

        // The host resolves and splits each URI; the engine types the cells per the declared schema.
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
