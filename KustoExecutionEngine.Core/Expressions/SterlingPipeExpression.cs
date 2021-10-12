using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class SterlingPipeExpression : SterlingExpression
    {
        public SterlingPipeExpression(SterlingEngine engine, PipeExpression expression)
            : base(engine, expression)
        {
            FilterOperator a;
        }

        protected override object EvaluateInternal()
        {
            // TODO: Return real data.
            return new EmptyTabularSource();
        }
    }
}
