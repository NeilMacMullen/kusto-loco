using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal sealed class StirlingFilterOperator : StirlingOperator<FilterOperator>
    {
        public StirlingFilterOperator(StirlingEngine engine, FilterOperator expression)
            : base(engine, expression)
        {
        }

        protected override ITableSource EvaluateTableInputInternal(ITableSource input)
        {
            // TODO implement filter
            return new DerivedTableSource(
                input,
                input.Schema,
                chunk =>
                {
                    return chunk;
                });
        }
    }
}
