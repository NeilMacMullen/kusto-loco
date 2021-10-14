using System;
using System.Collections.Generic;
using System.Linq;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.DataSource;

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
            //[Aggregates.Sum] = SumImpl,
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

        private object? SumImpl(object? input)
        {
            if (input is not IList<IRow> table)
            {
                throw new NotSupportedException($"Unexpected input type, expected IList<IRow>, got {TypeNameHelper.GetTypeDisplayName(input)}.");
            }

            // TODO: Implement sum
            //return table.Select(r => _argumentExpressions[0].Evaluate(r)).Aggregate((x, y) =>
            //    StirlingBinaryExpression.StirlingAddBinaryExpression.GetDecimalResultTypeImpl(_expressionResultType)
            //        .Invoke(x, y));
            return 0.0;

            throw new NotSupportedException($"Unexpected input type, expected IList<IRow>, got {TypeNameHelper.GetTypeDisplayName(input)}.");
        }
    }
}
