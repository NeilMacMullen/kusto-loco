using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal sealed class StirlingJoinOperator : StirlingOperator<JoinOperator>
    {
        public StirlingJoinOperator(StirlingEngine engine, JoinOperator projectOperator)
            : base(engine, projectOperator)
        {
        }

        protected override object? EvaluateTableInputInternal(ITabularSourceV2 input)
        {
            // TODO: Implement join
            return new EmptyTabularSourceV2();
        }
    }
}
