using System;
using System.Collections.Generic;
using Kusto.Language;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Expressions
{
    internal static class IntrinsicRowFunctions
    {
        internal delegate object? RowFunctionImpl(BabyKustoExpression[] argumentExpressions, IRow row);
        internal static readonly Dictionary<Symbol, RowFunctionImpl> RowFunctionsMap = new()
        {
            [Functions.ToLong] = ToLongImpl,
        };

        internal static bool TryGetImpl(Symbol functionSymbol, out RowFunctionImpl impl)
        {
            return RowFunctionsMap.TryGetValue(functionSymbol, out impl);
        }

        private static object? ToLongImpl(BabyKustoExpression[] argumentExpressions, object? input)
        {
            var argValue = argumentExpressions[0].Evaluate(input);
            return Convert.ToInt64(argValue);
        }
    }
}
