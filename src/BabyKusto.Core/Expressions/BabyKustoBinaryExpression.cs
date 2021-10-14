using System;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.Expressions
{
    internal class BabyKustoBinaryExpression : BabyKustoExpression
    {
        private readonly BabyKustoExpression _left;
        private readonly BabyKustoExpression _right;
        private readonly ExpressionMathHelper.BasicOperators.Impl _impl;

        public BabyKustoBinaryExpression(BabyKustoEngine engine, BinaryExpression expression)
            : base(engine, expression)
        {
            _left = BabyKustoExpression.Build(engine, expression.Left);
            _right = BabyKustoExpression.Build(engine, expression.Right);

            var operands = ExpressionMathHelper.GetOperandsByResultType(expression.ResultType.ToKustoValueKind());
            _impl = expression.Kind switch
            {
                SyntaxKind.AddExpression => operands.Add,
                SyntaxKind.SubtractExpression => operands.Subtract,
                SyntaxKind.DivideExpression => operands.Divide,
                SyntaxKind.MultiplyExpression => operands.Multiply,
                SyntaxKind.EqualExpression => operands.Equal,
                SyntaxKind.NotEqualExpression => operands.NotEqual,
                SyntaxKind.GreaterThanExpression => operands.GreaterThan,
                SyntaxKind.GreaterThanOrEqualExpression => operands.GreaterThanOrEqual,
                SyntaxKind.LessThanExpression => operands.LessThan,
                SyntaxKind.LessThanOrEqualExpression => operands.LessThanOrEqual,
                SyntaxKind.AndExpression => operands.And,
                SyntaxKind.OrExpression => operands.Or,
                _ => throw new InvalidOperationException($"Unsupported binary operation kind '{expression.Kind}'."),
            };

            if (_impl is null)
            {
                throw new InvalidOperationException($"Unsupported binary expression '{expression.Kind}' for result type '{expression.ResultType.ToKustoValueKind()}'.");
            }
        }

        protected override object? EvaluateInternal(object? input)
        {
            return _impl(_left.Evaluate(input), _right.Evaluate(input));
        }
    }
}
