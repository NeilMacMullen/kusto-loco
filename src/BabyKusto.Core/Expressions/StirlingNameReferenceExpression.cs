using System;
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

        protected override object? EvaluateRowInputInternal(IRow row)
        {
            // TODO: Shouldn't need to find column index for each row, as it's always the same...
            // Could we do it in the constructor?
            if (row.Schema.TryGetColumnIndex(Name, out var index))
            {
                return row.Values[index];
            }

            return EvaluateNullInputInternal();
        }

        protected override object? EvaluateNullInputInternal()
        {
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
