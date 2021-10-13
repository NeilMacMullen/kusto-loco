using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingParenthesizedExpression : StirlingExpression
    {
        private readonly StirlingExpression _inner;

        public StirlingParenthesizedExpression(StirlingEngine engine, ParenthesizedExpression expression)
            : base(engine, expression)
        {
            _inner = StirlingExpression.Build(engine, expression.Expression);
        }

        protected override object EvaluateInternal()
        {
            return _inner.Evaluate();
        }
    }
}

