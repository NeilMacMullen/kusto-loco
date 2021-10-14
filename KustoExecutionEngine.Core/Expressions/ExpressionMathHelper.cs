using System;
using KustoExecutionEngine.Core.DataSource;

namespace KustoExecutionEngine.Core.Expressions
{
    internal static class ExpressionMathHelper
    {
        private static BasicOperands DoubleOperands = new BasicOperands
        {
            Add = (left, right) => Convert.ToDouble(left) + Convert.ToDouble(right),
            Subtract = (left, right) => Convert.ToDouble(left) - Convert.ToDouble(right),
            Multiply = (left, right) => Convert.ToDouble(left) * Convert.ToDouble(right),
            Divide = (left, right) => Convert.ToDouble(left) / Convert.ToDouble(right),
        };

        private static BasicOperands LongOperands = new BasicOperands
        {
            Add = (left, right) => Convert.ToInt64(left) + Convert.ToInt64(right),
            Subtract = (left, right) => Convert.ToInt64(left) - Convert.ToInt64(right),
            Multiply = (left, right) => Convert.ToInt64(left) * Convert.ToInt64(right),
            Divide = (left, right) => Convert.ToInt64(left) / Convert.ToInt64(right),
        };

        private static BasicOperands IntOperands = new BasicOperands
        {
            Add = (left, right) => Convert.ToInt32(left) + Convert.ToInt32(right),
            Subtract = (left, right) => Convert.ToInt32(left) - Convert.ToInt32(right),
            Multiply = (left, right) => Convert.ToInt32(left) * Convert.ToInt32(right),
            Divide = (left, right) => Convert.ToInt32(left) / Convert.ToInt32(right),
        };

        internal static BasicOperands GetOperandsByResultType(KustoValueKind kind)
        {
            return kind switch
            {
                KustoValueKind.Real => DoubleOperands,
                KustoValueKind.Long => LongOperands,
                KustoValueKind.Int => IntOperands,
                _ => throw new InvalidOperationException($"Math operators not supported for type {kind}.")
            };
        }

        internal struct BasicOperands
        {
            internal delegate object? Impl(object? left, object? right);

            internal Impl Add;
            internal Impl Subtract;
            internal Impl Multiply;
            internal Impl Divide;
        }
    }
}
