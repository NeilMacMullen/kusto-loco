using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal abstract class SterlingBinaryExpression : SterlingExpression
    {
        private SterlingBinaryExpression(SterlingEngine engine, BinaryExpression expression)
            : base(engine, expression)
        {
        }

        internal static SterlingBinaryExpression Build(SterlingEngine engine, BinaryExpression expression)
        {
            return expression.Kind switch
            {
                SyntaxKind.AddExpression => new SterlingAddBinaryExpression(engine, expression),
                _ => throw new InvalidOperationException($"Unsupported binary operation kind '{expression.Kind}'."),
            };
        }

        private sealed class SterlingAddBinaryExpression
            : SterlingBinaryExpression
        {
            public SterlingAddBinaryExpression(SterlingEngine engine, BinaryExpression expression)
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
