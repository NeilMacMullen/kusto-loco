using Kusto.Language.Syntax;

namespace BabyKusto.Core.Expressions
{
    internal class BabyKustoSimpleNamedExpression : BabyKustoExpression
    {
        private readonly string _name;
        private readonly BabyKustoExpression _rightExpression;

        public BabyKustoSimpleNamedExpression(BabyKustoEngine engine, SimpleNamedExpression expression)
            : base(engine, expression)
        {
            _name = expression.Name.SimpleName;
            _rightExpression = BabyKustoExpression.Build(engine, expression.Expression);
        }

        public string Name => _name;

        protected override object? EvaluateInternal(object? input)
        {
            return _rightExpression.Evaluate(input);
        }
    }
}
