using System;
using Kusto.Language;
using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingFunctionCallExpression : StirlingExpression
    {
        private delegate object Impl(object? input);
        private readonly Impl _impl;

        public StirlingFunctionCallExpression(StirlingEngine engine, FunctionCallExpression expression)
            : base(engine, expression)
        {
            if (ReferenceEquals(expression.ReferencedSymbol, Functions.ToLong))
            {
                _impl = ToLongImpl;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported function {expression}.");
            }
        }

        protected override object EvaluateInternal(object? input)
        {
            return _impl(input);
        }

        private static object ToLongImpl(object? input)
        {
            //var value = 
            // TODO: Handle all possible input scalar data types
            return Convert.ToInt64(input);
        }
    }
}
