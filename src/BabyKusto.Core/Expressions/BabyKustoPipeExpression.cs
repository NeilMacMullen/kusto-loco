// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Syntax;

namespace BabyKusto.Core.Expressions
{
    internal class BabyKustoPipeExpression : BabyKustoExpression
    {
        private readonly BabyKustoExpression _leftExpression;
        private readonly BabyKustoExpression _operator;

        public BabyKustoPipeExpression(BabyKustoEngine engine, PipeExpression expression)
            : base(engine, expression)
        {
            _leftExpression = BabyKustoExpression.Build(engine, expression.Expression);
            _operator = BabyKustoExpression.Build(engine, expression.Operator);
        }

        protected override object? EvaluateInternal(object? input)
        {
            var leftValue = _leftExpression.Evaluate(input);
            return _operator.Evaluate(leftValue);
        }
    }
}
