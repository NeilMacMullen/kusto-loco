using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Operators
{
    internal sealed class StirlingProjectOperator : StirlingOperator<ProjectOperator>
    {
        public StirlingProjectOperator(StirlingEngine engine, ProjectOperator expression)
            : base(engine, expression)
        {
        }

        protected override ITabularSource EvaluateInternal(ITabularSource input)
        {
            // TODO: Implement project
            return new EmptyTabularSource();
        }
    }
}
