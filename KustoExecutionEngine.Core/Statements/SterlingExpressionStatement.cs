using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.Expressions;

namespace KustoExecutionEngine.Core.Statements
{
    internal class SterlingExpressionStatement : SterlingStatement<ExpressionStatement>
    {
        private readonly SterlingExpression _expression;

        public SterlingExpressionStatement(SterlingEngine engine, ExpressionStatement statement)
            : base(engine, statement)
        {
            _expression = SterlingExpression.Build(engine, statement.Expression);
        }

        protected override object ExecuteInternal()
        {
            return _expression.Evaluate();
        }
    }
}
