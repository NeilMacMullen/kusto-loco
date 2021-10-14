using System;
using System.Collections.Generic;
using Kusto.Language;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Expressions
{
    internal static class IntrinsicAggregationFunctions
    {
        internal delegate object? AggregationFunctionImpl(BabyKustoExpression[] argumentExpressions, ITableSource table);
        private static readonly Dictionary<Symbol, AggregationFunctionImpl> AggregationFunctionsMap = new()
        {
            [Aggregates.Count] = CountImpl,
            [Aggregates.Sum] = SumImpl,
            [Aggregates.Avg] = AvgImpl,
        };

        internal static bool TryGetImpl(Symbol functionSymbol, out AggregationFunctionImpl impl)
        {
            return AggregationFunctionsMap.TryGetValue(functionSymbol, out impl);
        }

        private static object? CountImpl(BabyKustoExpression[] argumentExpressions, ITableSource table)
        {
            long count = 0;
            foreach (var chunk in table.GetData())
            {
                count += chunk.RowCount;
            }

            return count;
        }

        private static object? SumImpl(BabyKustoExpression[] argumentExpressions, ITableSource table)
        {
            double sum = 0;
            foreach (var chunk in table.GetData())
            {
                for (int i = 0; i < chunk.RowCount; i++)
                {
                    var row = chunk.GetRow(i);
                    var value = argumentExpressions[0].Evaluate(row);
                    sum += Convert.ToDouble(value);
                }
            }

            return sum;
        }

        private static object? AvgImpl(BabyKustoExpression[] argumentExpressions, ITableSource table)
        {
            double sum = 0;
            long count = 0;
            foreach (var chunk in table.GetData())
            {
                count += chunk.RowCount;
                for (int i = 0; i < chunk.RowCount; i++)
                {
                    var row = chunk.GetRow(i);
                    var value = argumentExpressions[0].Evaluate(row);
                    sum += Convert.ToDouble(value);
                }
            }

            return sum / count;
        }
    }
}
