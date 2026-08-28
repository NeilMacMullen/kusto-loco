//
// Licensed under the MIT License.

using System;
using System.Linq;
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
        var resolver = context.Providers?.Get<IExternalDataResolver>();

        var columnNames = schema.Columns.Select(c => c.Name).ToArray();
        var columnTypes = schema.Columns.Select(c => TypeMapping.UnderlyingTypeForSymbol(c.Type)).ToArray();

        // No resolver registered => fail-closed (empty table); the host must opt in to fetch external data.
        var data = resolver?.Resolve(new ExternalDataRequest(node.Uris, node.Format, columnNames, columnTypes));

        var columns = new BaseColumn[schema.Columns.Count];
        for (var j = 0; j < schema.Columns.Count; j++)
        {
            var columnData = data != null && j < data.Count ? data[j] : Array.Empty<object?>();
            columns[j] = ColumnHelpers.CreateFromObjectArray(columnData, schema.Columns[j].Type);
        }

        return TabularResult.CreateUnvisualized(new InMemoryTableSource(schema, columns));
    }
}
