using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal sealed class StirlingSummarizeOperator : StirlingOperator<SummarizeOperator>
    {
        public StirlingSummarizeOperator(StirlingEngine engine, SummarizeOperator expression)
            : base(engine, expression)
        {
        }

        protected override ITabularSource EvaluateInternal(ITabularSource input)
        {
            // TODO: Implement summarize
            return new EmptyTabularSource();
        }
    }
}
