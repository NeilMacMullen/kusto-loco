using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.Expressions;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal sealed class StirlingProjectOperator : StirlingOperator<ProjectOperator>
    {
        private readonly List<(string ColumnName, StirlingExpression Expression)> _expressions;

        public StirlingProjectOperator(StirlingEngine engine, ProjectOperator projectOperator)
            : base(engine, projectOperator)
        {
            var resultType = projectOperator.ResultType;
            var expressions = projectOperator.Expressions;

            _expressions = new List<(string ColumnName, StirlingExpression Expression)>(expressions.Count);
            for (int i = 0; i < expressions.Count; i++)
            {
                var expression = expressions[i];
                var columnName = resultType.Members[i].Name;

                var builtExpression = StirlingExpression.Build(engine, expression.Element);
                _expressions.Add((columnName, builtExpression));
            }
        }

        protected override ITabularSource EvaluateInternal(ITabularSource input)
        {
            return new DerivedTabularSource(
                input,
                row =>
                {
                    // TODO: Shouldn't push context here, this would be passed as an arg to Evaluate below.
                    _engine.PushRowContext(row);
                    try
                    {
                        var values = new List<KeyValuePair<string, object?>>(_expressions.Count);
                        foreach (var expression in _expressions)
                        {
                            // TODO: Shouldn't pass the row here, instead should pass the table or a chunk of it...
                            var value = expression.Expression.Evaluate(row);
                            values.Add(new KeyValuePair<string, object?>(expression.ColumnName, value));
                        }
                        return new Row(values);
                    }
                    finally
                    {
                        _engine.LeaveExecutionContext();
                    }
                });
        }
    }
}
