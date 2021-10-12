using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.Operators;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingPipeExpression : StirlingExpression
    {
        private readonly StirlingExpression _leftExpression;
        private readonly StirlingOperator _operator;

        public StirlingPipeExpression(StirlingEngine engine, PipeExpression expression)
            : base(engine, expression)
        {
            _leftExpression = StirlingExpression.Build(engine, expression.Expression);
            _operator = StirlingOperator.Build(engine, expression.Operator);
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
