using System;
using System.Linq;
using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingFunctionCallExpression : StirlingExpression
    {
        private readonly IntrinsicRowFunctions.RowFunctionImpl _rowImpl = (_, _) => throw new NotSupportedException();
        private readonly IntrinsicAggregationFunctions.AggregationFunctionImpl _aggImpl = (_, _) => throw new NotSupportedException();

        private readonly StirlingExpression[] _argumentExpressions;

        public StirlingFunctionCallExpression(StirlingEngine engine, FunctionCallExpression expression)
            : base(engine, expression)
        {
            _argumentExpressions = expression.ArgumentList.Expressions
                .Select(e => StirlingExpression.Build(engine, e.Element))
                .ToArray();

            if (IntrinsicRowFunctions.TryGetImpl(expression.ReferencedSymbol, out var rowImpl))
            {
                _rowImpl = rowImpl;
            }
            else if (IntrinsicAggregationFunctions.TryGetImpl(expression.ReferencedSymbol, out var aggImpl))
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

        protected override object? EvaluateTableInputInternal(ITableSource table)
        {
            return _aggImpl(_argumentExpressions, table);
        }
    }
}
