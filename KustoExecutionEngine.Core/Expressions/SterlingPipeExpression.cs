using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class SterlingPipeExpression : SterlingExpression
    {
        private readonly SterlingExpression _leftExpression;
        private readonly SterlingOperatorExpression _operator;

        public SterlingPipeExpression(SterlingEngine engine, PipeExpression expression)
            : base(engine, expression)
        {
            _leftExpression = SterlingExpression.Build(engine, expression.Expression);
            _operator = SterlingOperatorExpression.Build(engine, expression.Operator);
        }

        protected override object EvaluateInternal()
        {
            var leftResult = _leftExpression.Evaluate();
            // TODO: How to pipe data to the operator? Perhaps Operator.Evaluate has wrong signature?
            return _operator.Evaluate();
        }
    }
}
