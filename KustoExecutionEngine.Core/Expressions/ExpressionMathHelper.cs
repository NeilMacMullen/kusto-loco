using System;

namespace KustoExecutionEngine.Core.Expressions
{
    internal static class ExpressionMathHelper
    {
        private static BasicOperators DoubleOperators = new BasicOperators
        {
            Add = (left, right) => Convert.ToDouble(left) + Convert.ToDouble(right),
            Subtract = (left, right) => Convert.ToDouble(left) - Convert.ToDouble(right),
            Multiply = (left, right) => Convert.ToDouble(left) * Convert.ToDouble(right),
            Divide = (left, right) => Convert.ToDouble(left) / Convert.ToDouble(right),
        };

        private static BasicOperators LongOperators = new BasicOperators
        {
            Add = (left, right) => Convert.ToInt64(left) + Convert.ToInt64(right),
            Subtract = (left, right) => Convert.ToInt64(left) - Convert.ToInt64(right),
            Multiply = (left, right) => Convert.ToInt64(left) * Convert.ToInt64(right),
            Divide = (left, right) => Convert.ToInt64(left) / Convert.ToInt64(right),
        };

        private static BasicOperators IntOperators = new BasicOperators
        {
            Add = (left, right) => Convert.ToInt32(left) + Convert.ToInt32(right),
            Subtract = (left, right) => Convert.ToInt32(left) - Convert.ToInt32(right),
            Multiply = (left, right) => Convert.ToInt32(left) * Convert.ToInt32(right),
            Divide = (left, right) => Convert.ToInt32(left) / Convert.ToInt32(right),
        };

        private static BasicOperators BoolOperators = new BasicOperators
        {
            Equal = (left, right) => Equals(left, right),
            NotEqual = (left, right) => !Equals(left, right),

            // TODO: This is obviously wrong. For boolean operators, picking the operator based solely on the result type (bool) is silly.
            GreaterThan = (left, right) => Convert.ToDouble(left) > Convert.ToDouble(right),
            GreaterThanOrEqual = (left, right) => Convert.ToDouble(left) >= Convert.ToDouble(right),
            LessThan = (left, right) => Convert.ToDouble(left) < Convert.ToDouble(right),
            LessThanOrEqual = (left, right) => Convert.ToDouble(left) <= Convert.ToDouble(right),
            And = (left, right) => Convert.ToBoolean(left) && Convert.ToBoolean(right),
            Or = (left, right) => Convert.ToBoolean(left) || Convert.ToBoolean(right),
        };

        internal static BasicOperators GetOperandsByResultType(KustoValueKind kind)
        {
            return kind switch
            {
                KustoValueKind.Real => DoubleOperators,
                KustoValueKind.Long => LongOperators,
                KustoValueKind.Int => IntOperators,
                KustoValueKind.Bool => BoolOperators,
                _ => throw new InvalidOperationException($"Math operators not supported for type {kind}.")
            };
        }

        internal struct BasicOperators
        {
            internal delegate object? Impl(object? left, object? right);

            internal Impl Add;
            internal Impl Subtract;
            internal Impl Multiply;
            internal Impl Divide;
            internal Impl Equal;
            internal Impl NotEqual;
            internal Impl GreaterThan;
            internal Impl GreaterThanOrEqual;
            internal Impl LessThan;
            internal Impl LessThanOrEqual;
            internal Impl And;
            internal Impl Or;
        }
    }
}
