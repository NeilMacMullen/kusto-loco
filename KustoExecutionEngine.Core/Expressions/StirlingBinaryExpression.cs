using System;
using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.DataSource;

namespace KustoExecutionEngine.Core.Expressions
{
    internal class StirlingBinaryExpression : StirlingExpression
    {
        private readonly StirlingExpression _left;
        private readonly StirlingExpression _right;
        private readonly ExpressionMathHelper.BasicOperands.Impl _impl;

        public StirlingBinaryExpression(StirlingEngine engine, BinaryExpression expression)
            : base(engine, expression)
        {
            _left = StirlingExpression.Build(engine, expression.Left);
            _right = StirlingExpression.Build(engine, expression.Right);

            var operands = ExpressionMathHelper.GetOperandsByResultType(expression.ResultType.ToKustoValueKind());
            _impl = expression.Kind switch
            {
                SyntaxKind.AddExpression => operands.Add,
                SyntaxKind.SubtractExpression => operands.Subtract,
                SyntaxKind.DivideExpression => operands.Divide,
                SyntaxKind.MultiplyExpression => operands.Multiply,
                _ => throw new InvalidOperationException($"Unsupported binary operation kind '{expression.Kind}'."),
            };
        }

        protected override object? EvaluateInternal(object? input)
        {
            return _impl(_left.Evaluate(input), _right.Evaluate(input));
        }
    }
}
