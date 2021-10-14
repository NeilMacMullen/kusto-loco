using System.Collections.Generic;
using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.DataSource;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal sealed class StirlingProjectOperator : StirlingOperator<ProjectOperator>
    {
        private readonly List<StirlingExpression> _expressions;
        private readonly TableSchema _resultSchema;

        public StirlingProjectOperator(StirlingEngine engine, ProjectOperator projectOperator)
            : base(engine, projectOperator)
        {
            var resultType = projectOperator.ResultType;
            var expressions = projectOperator.Expressions;

            _expressions = new List<StirlingExpression>(expressions.Count);
            var resultSchemaColumns = new List<ColumnDefinition>(expressions.Count);
            for (int i = 0; i < expressions.Count; i++)
            {
                var expression = expressions[i].Element;
                var columnName = resultType.Members[i].Name;
                var columnValueType = expression.ResultType.ToKustoValueKind();

                var builtExpression = StirlingExpression.Build(engine, expression);
                _expressions.Add(builtExpression);

                resultSchemaColumns.Add(new ColumnDefinition(columnName, columnValueType));
            }

            _resultSchema = new TableSchema(resultSchemaColumns);
        }

        protected override ITabularSourceV2 EvaluateTableInputInternal(ITabularSourceV2 input)
        {
            return new DerivedTabularSourceV2(
                input,
                _resultSchema,
                chunk =>
                {
                    var columns = new Column[_expressions.Count];
                    for (var i = 0; i < _expressions.Count; i++)
                    {
                        columns[i] = OperatorHelpers.ProjectColumn(_engine, chunk, _expressions[i]);
                    }

                    return new TableChunk(_resultSchema, columns);
                });
        }
    }
}
