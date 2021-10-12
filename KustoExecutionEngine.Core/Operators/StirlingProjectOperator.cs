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
            return new DerivedTabularSource(
                input,
                row =>
                {
                    // TODO: Implement project
                    return row;
                });
        }
    }
}
