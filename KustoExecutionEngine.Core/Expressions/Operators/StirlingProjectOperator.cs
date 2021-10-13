using System.Collections.Generic;
using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.DataSource;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal sealed class StirlingProjectOperator : StirlingOperator<ProjectOperator>
    {
        private readonly List<(string ColumnName, KustoValueKind ColumnValueType, StirlingExpression Expression)> _expressions;

        public StirlingProjectOperator(StirlingEngine engine, ProjectOperator projectOperator)
            : base(engine, projectOperator)
        {
            var resultType = projectOperator.ResultType;
            var expressions = projectOperator.Expressions;

            _expressions = new List<(string ColumnName, KustoValueKind ColumnValueType, StirlingExpression Expression)>(expressions.Count);
            for (int i = 0; i < expressions.Count; i++)
            {
                var expression = expressions[i].Element;
                var columnName = resultType.Members[i].Name;
                var columnValueType = expression.ResultType.ToKustoValueKind();

                var builtExpression = StirlingExpression.Build(engine, expression);
                _expressions.Add((columnName, columnValueType, builtExpression));
            }
        }

        protected override ITabularSourceV2 EvaluateTableInputInternal(ITabularSourceV2 input)
        {
            var newColumnDefinitions = new List<ColumnDefinition>();
            foreach (var expression in _expressions)
            {
                newColumnDefinitions.Add(new ColumnDefinition(expression.ColumnName, expression.ColumnValueType));
            }
            var newSchema = new TableSchema(newColumnDefinitions);
            return new DerivedTabularSourceV2(
                input,
                newSchema,
                oldTableChunk =>
                {
                    var columns = new Column[newSchema.ColumnDefinitions.Count];
                    for (var i = 0; i < columns.Length; i++)
                    {
                        columns[i] = new Column(oldTableChunk.Columns[0].Size);
                    }

                    var newTableChunk = new TableChunk(newSchema, columns);
                    for (int i = 0; i < oldTableChunk.Columns[0].Size; i++)
                    {
                        _engine.PushRowContext(oldTableChunk.GetRow(i));
                        try
                        {
                            var values = new List<KeyValuePair<string, object?>>(_expressions.Count);
                            foreach (var expression in _expressions)
                            {
                                var value = expression.Expression.Evaluate(null);
                                values.Add(new KeyValuePair<string, object?>(expression.ColumnName, value));
                            }

                            newTableChunk.SetRow(new Row(values), i);
                        }
                        finally
                        {
                            _engine.LeaveExecutionContext();
                        }
                    }

                    return newTableChunk;
                });
        }
    }
}
