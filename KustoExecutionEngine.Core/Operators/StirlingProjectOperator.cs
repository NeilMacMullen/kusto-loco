using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.Expressions;

namespace KustoExecutionEngine.Core.Operators
{
    internal sealed class StirlingProjectOperator : StirlingOperator<ProjectOperator>
    {
        private readonly List<StirlingExpression> _expressions;

        public StirlingProjectOperator(StirlingEngine engine, ProjectOperator projectOperator)
            : base(engine, projectOperator)
        {
            _expressions = new List<StirlingExpression>(projectOperator.Expressions.Count);
            foreach (var expression in projectOperator.Expressions)
            {
                _expressions.Add(StirlingExpression.Build(engine, expression.Element));
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
                            string name;
                            if (expression is StirlingNameReferenceExpression nameReferenceExpression)
                            {
                                name = nameReferenceExpression.Name;
                            }
                            else if (expression is StirlingSimpleNamedExpression simpleNamedExpression)
                            {
                                name = simpleNamedExpression.Name;
                            }
                            else
                            {
                                throw new InvalidOperationException($"Unsupported expression type inside project operator: {TypeNameHelper.GetTypeDisplayName(expression)}");
                            }

                            var value = expression.Evaluate();
                            values.Add(new KeyValuePair<string, object?>(name, value));
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
