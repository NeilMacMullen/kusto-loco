using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal sealed class StirlingFilterOperator : StirlingOperator<FilterOperator>
    {
        public StirlingFilterOperator(StirlingEngine engine, FilterOperator expression)
            : base(engine, expression)
        {
        }

        protected override ITabularSource EvaluateInternal(ITabularSource input)
        {
            return new DerivedTabularSource(
                input,
                row =>
                {
                    if (SelectThisRow(row))
                    {
                        return row;
                    }

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
