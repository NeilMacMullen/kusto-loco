// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Syntax;

namespace BabyKusto.Core.Expressions
{
    internal class BabyKustoParenthesizedExpression : BabyKustoExpression
    {
        private readonly BabyKustoExpression _inner;

        public BabyKustoParenthesizedExpression(BabyKustoEngine engine, ParenthesizedExpression expression)
            : base(engine, expression)
        {
            _inner = BabyKustoExpression.Build(engine, expression.Expression);
        }

        protected override object? EvaluateInternal(object? input)
        {
            return _inner.Evaluate(input);
        }
    }
}

