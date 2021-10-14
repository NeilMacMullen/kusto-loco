using BabyKusto.Core.Expressions;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.Statements
{
    internal class BabyKustoExpressionStatement : BabyKustoStatement<ExpressionStatement>
    {
        private readonly BabyKustoExpression _expression;

        public BabyKustoExpressionStatement(BabyKustoEngine engine, ExpressionStatement statement)
            : base(engine, statement)
        {
            _expression = BabyKustoExpression.Build(engine, statement.Expression);
        }

        protected override object? ExecuteInternal()
        {
            // TODO: Shouldn't be hardcoding `null` below
            return _expression.Evaluate(null);
        }
    }
}
