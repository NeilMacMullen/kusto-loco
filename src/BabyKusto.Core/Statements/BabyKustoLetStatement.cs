using BabyKusto.Core.Expressions;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.Statements
{
    internal class BabyKustoLetStatement : BabyKustoStatement<LetStatement>
    {
        private readonly string _name;
        private readonly BabyKustoExpression _expression;

        public BabyKustoLetStatement(BabyKustoEngine engine, LetStatement statement)
            : base(engine, statement)
        {
            _name = statement.Name.SimpleName;
            _expression = BabyKustoExpression.Build(engine, statement.Expression);
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
