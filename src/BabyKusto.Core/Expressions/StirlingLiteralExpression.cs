using System;
using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal abstract class StirlingLiteralExpression : StirlingExpression
    {
        private StirlingLiteralExpression(StirlingEngine engine, LiteralExpression expression)
            : base(engine, expression)
        {
        }

        internal static StirlingLiteralExpression Build(StirlingEngine engine, LiteralExpression expression)
        {
            return expression.Kind switch
            {
                SyntaxKind.BooleanLiteralExpression or
                SyntaxKind.IntLiteralExpression or
                SyntaxKind.LongLiteralExpression or
                SyntaxKind.RealLiteralExpression or
                SyntaxKind.DecimalLiteralExpression or
                SyntaxKind.DateTimeLiteralExpression or
                SyntaxKind.TimespanLiteralExpression or
                SyntaxKind.GuidLiteralExpression or
                SyntaxKind.StringLiteralExpression or
                SyntaxKind.NullLiteralExpression or
                SyntaxKind.LongLiteralExpression => new StirlingLiteralAnyExpression(engine, expression),
                _ => throw new InvalidOperationException($"Unsupported literal expression kind '{expression.Kind}'."),
            };
        }

        private sealed class StirlingLiteralAnyExpression : StirlingLiteralExpression
        {
            private readonly object _value;

            public StirlingLiteralAnyExpression(StirlingEngine engine, LiteralExpression expression)
                : base(engine, expression)
            {
                _value = expression.LiteralValue;
            }

            protected override object? EvaluateInternal(object? input)
            {
                return _value;
            }
        }
    }
}
