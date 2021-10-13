using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingDataTableExpressions : StirlingExpression
    {
        public StirlingDataTableExpressions(StirlingEngine engine, DataTableExpression expression)
            : base(engine, expression)
        {
            throw new NotImplementedException();
        }

        protected override object EvaluateInternal()
        {
        }
    }
}
