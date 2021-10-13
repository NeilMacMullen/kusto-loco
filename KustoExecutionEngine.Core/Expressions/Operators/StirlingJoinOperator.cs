using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.DataSource;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal sealed class StirlingJoinOperator : StirlingOperator<JoinOperator>
    {
        public StirlingJoinOperator(StirlingEngine engine, JoinOperator projectOperator)
            : base(engine, projectOperator)
        {
        }

        protected override ITabularSourceV2 EvaluateInternal(ITabularSourceV2 input)
        {
            // TODO: Implement join
            return new EmptyTabularSourceV2();
        }
    }
}
