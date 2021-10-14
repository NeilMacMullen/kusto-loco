using System.Collections.Generic;
using KustoExecutionEngine.Core.DataSource;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal static class OperatorHelpers
    {
        internal static Column ProjectColumn(StirlingEngine engine, ITableChunk chunk, StirlingExpression expression)
        {
            var result = new Column(chunk.RowCount);
            for (int i = 0; i < chunk.RowCount; i++)
            {
                engine.PushRowContext(chunk.GetRow(i));
                try
                {
                    var value = expression.Evaluate(null);

                    result[i] = value;
                }
                finally
                {
                    engine.LeaveExecutionContext();
                }
            }

            return result;
        }

        internal static ITabularSourceV2 ProjectColumn(StirlingEngine engine, ITabularSourceV2 source, StirlingExpression expression, string? name = null)
        {
            return new ProjectColumnTable(engine, source, expression, name);
        }

        private class ProjectColumnTable : ITabularSourceV2
        {
            private readonly StirlingEngine _engine;
            private readonly ITabularSourceV2 _source;
            private readonly StirlingExpression _expression;
            private readonly TableSchema _schema;

            public ProjectColumnTable(StirlingEngine engine, ITabularSourceV2 source, StirlingExpression expression, string? name = null)
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
