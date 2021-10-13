using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.Expressions;

namespace KustoExecutionEngine.Core.Statements
{
    internal class StirlingExpressionStatement : StirlingStatement<ExpressionStatement>
    {
        private readonly StirlingExpression _expression;

        public StirlingExpressionStatement(StirlingEngine engine, ExpressionStatement statement)
            : base(engine, statement)
        {
            _expression = StirlingExpression.Build(engine, statement.Expression);
        }

        protected override object? ExecuteInternal()
        {
            return _expression.Evaluate();
        }
    }
}
