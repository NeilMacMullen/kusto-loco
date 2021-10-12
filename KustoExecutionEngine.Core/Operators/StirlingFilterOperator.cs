using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Operators
{
    internal sealed class StirlingFilterOperator : StirlingOperator
    {
        public StirlingFilterOperator(StirlingEngine engine, FilterOperator expression)
            : base(engine, expression)
        {
        }

        protected override ITabularSource EvaluateInternal(ITabularSource input)
        {
            // TODO: Implement filter
            return new EmptyTabularSource();
        }
    }
}
