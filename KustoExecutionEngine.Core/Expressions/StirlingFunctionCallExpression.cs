using System;
using System.Collections.Generic;
using System.Linq;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingFunctionCallExpression : StirlingExpression
    {
        private delegate object? RowFunctionImpl(StirlingExpression[] argumentExpressions, IRow row);
        private static readonly Dictionary<Symbol, RowFunctionImpl> RowFunctionsMap = new()
        {
            [Functions.ToLong] = ToLongImpl,
        };

        private delegate object? AggregationFunctionImpl(StirlingExpression[] argumentExpressions, ITabularSourceV2 table);
        private static readonly Dictionary<Symbol, AggregationFunctionImpl> AggregationFunctionsMap = new()
        {
            [Aggregates.Count] = CountImpl,
            [Aggregates.Sum] = SumImpl,
            [Aggregates.Avg] = AvgImpl,
        };

        private readonly RowFunctionImpl _rowImpl = (_, _) => throw new NotSupportedException();
        private readonly AggregationFunctionImpl _aggImpl = (_, _) => throw new NotSupportedException();

        private readonly StirlingExpression[] _argumentExpressions;

        public StirlingFunctionCallExpression(StirlingEngine engine, FunctionCallExpression expression)
            : base(engine, expression)
        {
            _argumentExpressions = expression.ArgumentList.Expressions
                .Select(e => StirlingExpression.Build(engine, e.Element))
                .ToArray();

            if (RowFunctionsMap.TryGetValue(expression.ReferencedSymbol, out var rowImpl))
            {
                _rowImpl = rowImpl;
            }
            else if (AggregationFunctionsMap.TryGetValue(expression.ReferencedSymbol, out var aggImpl))
            {
                _aggImpl = aggImpl;
            }
            else
            {
                // TODO: Support user functions
                throw new InvalidOperationException($"Unsupported function {expression}.");
            }
        }

        protected override object? EvaluateRowInputInternal(IRow row)
        {
            return _rowImpl(_argumentExpressions, row);
        }

        protected override object? EvaluateTableInputInternal(ITabularSourceV2 table)
        {
            return _aggImpl(_argumentExpressions, table);
        }

        private static object? ToLongImpl(StirlingExpression[] argumentExpressions, object? input)
        {
            var argValue = argumentExpressions[0].Evaluate(input);
            return Convert.ToInt64(argValue);
        }

        private static object? CountImpl(StirlingExpression[] argumentExpressions, ITabularSourceV2 table)
        {
            long count = 0;
            foreach (var chunk in table.GetData())
            {
                count += chunk.RowCount;
            }

            return count;
        }

        private static object? SumImpl(StirlingExpression[] argumentExpressions, ITabularSourceV2 table)
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

        private static object? AvgImpl(StirlingExpression[] argumentExpressions, ITabularSourceV2 table)
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
