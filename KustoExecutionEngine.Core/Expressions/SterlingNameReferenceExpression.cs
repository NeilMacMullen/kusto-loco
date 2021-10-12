using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class SterlingNameReferenceExpression : SterlingExpression
    {
        public SterlingNameReferenceExpression(SterlingEngine engine, NameReference expression)
            : base(engine, expression)
        {
        }

        protected override object EvaluateInternal()
        {
            // TODO: Return real data.
            return new EmptyTabularSource();
        }
    }
}
