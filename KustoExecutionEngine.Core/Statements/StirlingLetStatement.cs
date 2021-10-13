using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.Expressions;

namespace KustoExecutionEngine.Core.Statements
{
    internal class StirlingLetStatement : StirlingStatement<LetStatement>
    {
        private readonly string _name;
        private readonly StirlingExpression _expression;

        public StirlingLetStatement(StirlingEngine engine, LetStatement statement)
            : base(engine, statement)
        {
            _name = statement.Name.SimpleName;
            _expression = StirlingExpression.Build(engine, statement.Expression);
        }

        public string Name => _name;

        protected override object? ExecuteInternal()
        {
            // TODO: Shouldn't be hardcoding `null` below
            var value = _expression.Evaluate(null);
            _engine.PushDeclarationsContext(Name, value);
            return null;
        }
    }
}
