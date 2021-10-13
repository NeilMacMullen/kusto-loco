using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingSimpleNamedExpression : StirlingExpression
    {
        private readonly string _name;
        private readonly StirlingExpression _rightExpression;

        public StirlingSimpleNamedExpression(StirlingEngine engine, SimpleNamedExpression expression)
            : base(engine, expression)
        {
            _name = expression.Name.SimpleName;
            _rightExpression = StirlingExpression.Build(engine, expression.Expression);
        }

        public string Name => _name;

        protected override object? EvaluateInternal(object? input)
        {
            return _rightExpression.Evaluate(input);
        }
    }
}
