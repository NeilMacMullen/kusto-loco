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
                    // TODO: Implement project
                    return row;
                });
        }
    }
}
