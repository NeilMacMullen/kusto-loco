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

        protected override ITabularSourceV2 EvaluateInternal(ITabularSourceV2 input)
        {
            // TODO implement filter
            return new DerivedTabularSourceV2(
                input,
                input.Schema,
                tableChunk =>
                {
                    return null;
                });

            bool SelectThisRow(IRow row)
            {
                // TODO: Implement filter
                return true;
            }
        }
    }
}
