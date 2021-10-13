using System;
using System.Collections.Generic;
using System.Linq;
using Kusto.Language;
using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingFunctionCallExpression : StirlingExpression
    {
        private delegate object Impl(object? input);
        private readonly Impl _impl;

        private readonly StirlingExpression[] _argumentExpressions;

        public StirlingFunctionCallExpression(StirlingEngine engine, FunctionCallExpression expression)
            : base(engine, expression)
        {
            _argumentExpressions = expression.ArgumentList.Expressions
                .Select(e => StirlingExpression.Build(engine, e.Element))
                .ToArray();

            if (ReferenceEquals(expression.ReferencedSymbol, Functions.ToLong))
            {
                CheckExpectedArgumentsCount(1);
                _impl = ToLongImpl;
            }
            else if (ReferenceEquals(expression.ReferencedSymbol, Aggregates.Count))
            {
                CheckExpectedArgumentsCount(0);
                _impl = CountImpl;
            }
            else if (ReferenceEquals(expression.ReferencedSymbol, Aggregates.Sum))
            {
                CheckExpectedArgumentsCount(1);
                _impl = SumImpl;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported function {expression}.");
            }

            void CheckExpectedArgumentsCount(int expected)
            {
                if (_argumentExpressions.Length != expected)
                {
                    throw new InvalidOperationException($"Mismatched arguments count. Function {expression.Name} expects {expected}, found {_argumentExpressions.Length}.");
                }
            }
        }

        protected override object EvaluateInternal(object? input)
        {
            return _impl(input);
        }

        private object ToLongImpl(object? input)
        {
            var argValue = _argumentExpressions[0].Evaluate(input);
            return Convert.ToInt64(argValue);
        }

        private object CountImpl(object? input)
        {
            if (input is not IList<IRow> table)
            {
                throw new NotSupportedException($"Unexpected input type, expected IList<IRow>, got {TypeNameHelper.GetTypeDisplayName(input)}.");
            }

            return table.Count;
        }

        private object SumImpl(object? input)
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
