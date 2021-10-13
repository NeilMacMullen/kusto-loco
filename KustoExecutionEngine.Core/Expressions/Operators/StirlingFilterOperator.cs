using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.DataSource;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal sealed class StirlingFilterOperator : StirlingOperator<FilterOperator>
    {
        public StirlingFilterOperator(StirlingEngine engine, FilterOperator expression)
            : base(engine, expression)
        {
        }

        protected override ITabularSourceV2 EvaluateTableInputInternal(ITabularSourceV2 input)
        {
            // TODO implement filter
            return new DerivedTabularSourceV2(
                input,
                input.Schema,
                chunk =>
                {
                    return chunk;
                });
        }
    }
}
