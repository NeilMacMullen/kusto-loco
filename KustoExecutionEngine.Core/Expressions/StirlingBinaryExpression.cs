using Kusto.Language.Symbols;
using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal abstract class StirlingBinaryExpression : StirlingExpression
    {
        private StirlingBinaryExpression(StirlingEngine engine, BinaryExpression expression)
            : base(engine, expression)
        {
        }

        internal static StirlingBinaryExpression Build(StirlingEngine engine, BinaryExpression expression)
        {
            return expression.Kind switch
            {
                SyntaxKind.AddExpression => new StirlingAddBinaryExpression(engine, expression),
                SyntaxKind.SubtractExpression => new StirlingSubtractBinaryExpression(engine, expression),
                SyntaxKind.DivideExpression => new StirlingMultiplyBinaryExpression(engine, expression),
                SyntaxKind.MultiplyExpression => new StirlingMultiplyBinaryExpression(engine, expression),
                _ => throw new InvalidOperationException($"Unsupported binary operation kind '{expression.Kind}'."),
            };
        }

        private sealed class StirlingAddBinaryExpression : StirlingBinaryExpression
        {
            delegate object AddImpl(object leftVal, object rightVal);

            private readonly StirlingExpression _left;
            private readonly StirlingExpression _right;
            private readonly AddImpl _impl;

            public StirlingAddBinaryExpression(StirlingEngine engine, BinaryExpression expression)
                : base(engine, expression)
            {
                _left = StirlingExpression.Build(engine, expression.Left);
                _right = StirlingExpression.Build(engine, expression.Right);

                if (ReferenceEquals(expression.ResultType, ScalarTypes.Real))
                {
                    _impl = ImplDouble;
                }
                else if (ReferenceEquals(expression.ResultType, ScalarTypes.Long))
                {
                    _impl = ImplLong;
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported return type {expression.ResultType}.");
                }
            }

            protected override object EvaluateInternal(object? input)
            {
                var leftVal = _left.Evaluate(input);
                var rightVal = _right.Evaluate(input);
                return _impl(leftVal, rightVal);
            }

            static object ImplDouble(object leftVal, object rightVal) => Convert.ToDouble(leftVal) + Convert.ToDouble(rightVal);
            static object ImplLong(object leftVal, object rightVal) => Convert.ToInt64(leftVal) + Convert.ToInt64(rightVal);
        }

        private sealed class StirlingSubtractBinaryExpression : StirlingBinaryExpression
        {
            public StirlingSubtractBinaryExpression(StirlingEngine engine, BinaryExpression expression)
                : base(engine, expression)
            {
            }

            protected override object EvaluateInternal(object? input)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class StirlingDivideBinaryExpression : StirlingBinaryExpression
        {
            public StirlingDivideBinaryExpression(StirlingEngine engine, BinaryExpression expression)
                : base(engine, expression)
            {
            }

            protected override object EvaluateInternal(object? input)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class StirlingMultiplyBinaryExpression : StirlingBinaryExpression
        {
            public StirlingMultiplyBinaryExpression(StirlingEngine engine, BinaryExpression expression)
                : base(engine, expression)
            {
            }

            protected override object EvaluateInternal(object? input)
            {
                throw new NotImplementedException();
            }
        }
    }
}
