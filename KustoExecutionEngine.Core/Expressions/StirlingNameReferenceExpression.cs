using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingNameReferenceExpression : StirlingExpression
    {
        public StirlingNameReferenceExpression(StirlingEngine engine, NameReference expression)
            : base(engine, expression)
        {
        }

        protected override object EvaluateInternal()
        {
            // TODO: Return real data.
            return new EmptyTabularSource();
        }
    }
}
