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

        private sealed class StirlingAddBinaryExpression
            : StirlingBinaryExpression
        {
            public StirlingAddBinaryExpression(StirlingEngine engine, BinaryExpression expression)
                : base(engine, expression)
            {
            }

            protected override object EvaluateInternal()
            {
                throw new NotImplementedException();
            }
        }

        private sealed class StirlingSubtractBinaryExpression
            : StirlingBinaryExpression
        {
            public StirlingSubtractBinaryExpression(StirlingEngine engine, BinaryExpression expression)
                : base(engine, expression)
            {
            }

            protected override object EvaluateInternal()
            {
                throw new NotImplementedException();
            }
        }

        private sealed class StirlingDivideBinaryExpression
            : StirlingBinaryExpression
        {
            public StirlingDivideBinaryExpression(StirlingEngine engine, BinaryExpression expression)
                : base(engine, expression)
            {
            }

            protected override object EvaluateInternal()
            {
                throw new NotImplementedException();
            }
        }

        private sealed class StirlingMultiplyBinaryExpression
            : StirlingBinaryExpression
        {
            public StirlingMultiplyBinaryExpression(StirlingEngine engine, BinaryExpression expression)
                : base(engine, expression)
            {
            }

            protected override object EvaluateInternal()
            {
                throw new NotImplementedException();
            }
        }
    }
}
