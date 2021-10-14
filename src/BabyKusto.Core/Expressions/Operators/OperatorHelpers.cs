using System.Collections.Generic;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal static class OperatorHelpers
    {
        internal static Column ProjectColumn(StirlingEngine engine, ITableChunk chunk, StirlingExpression expression)
        {
            var data = new object?[chunk.RowCount];
            for (int i = 0; i < chunk.RowCount; i++)
            {
                var row = chunk.GetRow(i);
                var value = expression.Evaluate(row);
                data[i] = value;
            }

            return new Column(data);
        }

        internal static ITableSource ProjectColumn(StirlingEngine engine, ITableSource source, StirlingExpression expression, string? name = null)
        {
            return new ProjectColumnTable(engine, source, expression, name);
        }

        private class ProjectColumnTable : ITableSource
        {
            private readonly StirlingEngine _engine;
            private readonly ITableSource _source;
            private readonly StirlingExpression _expression;
            private readonly TableSchema _schema;

            public ProjectColumnTable(StirlingEngine engine, ITableSource source, StirlingExpression expression, string? name = null)
            {
                _engine = engine;
                _source = source;
                _expression = expression;
                _schema = new TableSchema(
                    new List<ColumnDefinition>
                    {
                        new ColumnDefinition(name ?? "Column1", expression._expression.ResultType.ToKustoValueKind()),
                    });
            }

            public TableSchema Schema => _schema;

            public IEnumerable<ITableChunk> GetData()
            {
                foreach (var chunk in _source.GetData())
                {
                    var columnData = ProjectColumn(_engine, chunk, _expression);
                    yield return new TableChunk(_schema, new Column[] { columnData });
                }
            }
        }
    }
}
