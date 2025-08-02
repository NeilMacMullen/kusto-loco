using System.Diagnostics;
using System.Linq;
using KustoLoco.Core.Util;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitGetSchemaOperator(IRGetSchemaOperatorNode node, EvaluationContext context)
    {
        Debug.Assert(context.Left != TabularResult.Empty);


        var cs = new[]
        {
            new ColumnSymbol("ColumnName", ScalarTypes.String),
            new ColumnSymbol("ColumnOrdinal", ScalarTypes.Int),
            new ColumnSymbol("DataType", ScalarTypes.String),
            new ColumnSymbol("ColumnType", ScalarTypes.String),
        };
        var ts = new TableSymbol("schema", cs);
        var builders = new BaseColumnBuilder[]
        {
            new GenericColumnBuilderOfstring(),
            new GenericColumnBuilderOfint(),
            new GenericColumnBuilderOfstring(),
            new GenericColumnBuilderOfstring()
        };
        var table = context.Left.Value;
        var i = 0;
        foreach (var col in table.Type.Columns)
        {
            builders[0].Add(col.Name);
            builders[1].Add(i);
            builders[2].Add(col.Type.Name);
            builders[3].Add(col.Type.Name);
            i++;
        }

        var schema = new InMemoryTableSource(ts, builders.Select(b => b.ToColumn()).ToArray())
            ;
        return TabularResult.CreateWithVisualisation(schema, context.Left.VisualizationState);
    }
}
