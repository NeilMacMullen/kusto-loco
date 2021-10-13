using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Operators
{
    internal sealed class StirlingJoinOperator : StirlingOperator<JoinOperator>
    {
        public StirlingJoinOperator(StirlingEngine engine, JoinOperator projectOperator)
            : base(engine, projectOperator)
        {
        }

        protected override ITabularSource EvaluateInternal(ITabularSource input)
        {
            // TODO: Implement join
            return new DerivedTabularSource(
                input,
                row =>
                {
                    return row;
                });
        }
    }
}
