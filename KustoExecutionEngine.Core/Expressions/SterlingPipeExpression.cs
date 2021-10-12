using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.Operators;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class SterlingPipeExpression : SterlingExpression
    {
        private readonly SterlingExpression _leftExpression;
        private readonly SterlingOperator _operator;

        public SterlingPipeExpression(SterlingEngine engine, PipeExpression expression)
            : base(engine, expression)
        {
            _leftExpression = SterlingExpression.Build(engine, expression.Expression);
            _operator = SterlingOperator.Build(engine, expression.Operator);
        }

        protected override object EvaluateInternal()
        {
            var input = _leftExpression.Evaluate();
            if (input is not ITabularSource tabularSource)
            {
                throw new InvalidOperationException($"Pipe expression expects left side to produce tabular data.");
            }

            return _operator.Evaluate(tabularSource);
        }
    }
}
