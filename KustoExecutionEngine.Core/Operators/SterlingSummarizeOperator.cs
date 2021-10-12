using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Operators
{
    internal sealed class SterlingSummarizeOperator : SterlingOperator
    {
        public SterlingSummarizeOperator(SterlingEngine engine, SummarizeOperator expression)
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
