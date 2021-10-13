using System;
using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingDataTableExpression : StirlingExpression
    {
        public StirlingDataTableExpression(StirlingEngine engine, DataTableExpression expression)
            : base(engine, expression)
        {
            throw new NotImplementedException();
        }

        protected override object EvaluateInternal(object? input)
        {
            throw new NotImplementedException();
        }
    }
}
