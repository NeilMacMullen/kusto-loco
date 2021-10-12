using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.Expressions;

namespace KustoExecutionEngine.Core.Operators
{
    internal sealed class StirlingProjectOperator : StirlingOperator<ProjectOperator>
    {
        private readonly List<(string ColumnName, StirlingExpression Expression)> _expressions;

        public StirlingProjectOperator(StirlingEngine engine, ProjectOperator projectOperator)
            : base(engine, projectOperator)
        {
            var usedColumnNames = new HashSet<string>();
            _expressions = new List<(string ColumnName, StirlingExpression Expression)>(projectOperator.Expressions.Count);
            int generatedColumns = 0;
            foreach (var expression in projectOperator.Expressions)
            {
                var builtExpression = StirlingExpression.Build(engine, expression.Element);

                string? columnName = null;
                if (builtExpression is StirlingNameReferenceExpression nameReferenceExpression)
                {
                    columnName = nameReferenceExpression.Name;
                }
                else if (builtExpression is StirlingSimpleNamedExpression simpleNamedExpression)
                {
                    columnName = simpleNamedExpression.Name;
                }
                else
                {
                    columnName = $"Column{++generatedColumns}";
                }

                if (!usedColumnNames.Add(columnName))
                {
                    int suffix = 1;
                    string attempt;
                    do
                    {
                        attempt = $"{columnName}{suffix++}";
                    } while (!usedColumnNames.Add(attempt));
                    columnName = attempt;
                }

                _expressions.Add((columnName, builtExpression));
            }
        }

        protected override ITabularSource EvaluateInternal(ITabularSource input)
        {
            return new DerivedTabularSource(
                input,
                row =>
                {
                    _engine.PushRowContext(row);
                    try
                    {
                        var values = new List<KeyValuePair<string, object?>>(_expressions.Count);
                        foreach (var expression in _expressions)
                        {
                            var value = expression.Expression.Evaluate();
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
