using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingPipeExpression : StirlingExpression
    {
        private readonly StirlingExpression _leftExpression;
        private readonly StirlingExpression _operator;

        public StirlingPipeExpression(StirlingEngine engine, PipeExpression expression)
            : base(engine, expression)
        {
            _leftExpression = StirlingExpression.Build(engine, expression.Expression);
            _operator = StirlingExpression.Build(engine, expression.Operator);
        }

        protected override object? EvaluateInternal(object? input)
        {
            var leftValue = _leftExpression.Evaluate(input);
            return _operator.Evaluate(leftValue);
        }
    }
}
