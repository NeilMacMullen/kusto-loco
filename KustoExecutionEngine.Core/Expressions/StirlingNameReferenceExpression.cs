using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingNameReferenceExpression : StirlingExpression
    {
        public StirlingNameReferenceExpression(StirlingEngine engine, NameReference expression)
            : base(engine, expression)
        {
        }

        public string Name => ((NameReference)_expression).Name.SimpleName;

        protected override object EvaluateInternal(object? input)
        {
            // TODO: This should do something different when `input` is provided!

            if (!_engine.ExecutionContext.TryGetBinding(Name, out var value))
            {
                throw new InvalidOperationException($"Could not find binding for name '{Name}' in current scope.");
            }

            if (value is null)
            {
                // TODO: Is this always bad?
                throw new InvalidOperationException($"Found null value for name '{Name}' in current scope.");
            }

            return value;
        }
    }
}
