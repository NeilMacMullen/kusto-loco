// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.Expressions
{
    internal abstract class BabyKustoLiteralExpression : BabyKustoExpression
    {
        private BabyKustoLiteralExpression(BabyKustoEngine engine, LiteralExpression expression)
            : base(engine, expression)
        {
        }

        internal static BabyKustoLiteralExpression Build(BabyKustoEngine engine, LiteralExpression expression)
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
                SyntaxKind.LongLiteralExpression => new BabyKustoLiteralAnyExpression(engine, expression),
                _ => throw new InvalidOperationException($"Unsupported literal expression kind '{expression.Kind}'."),
            };
        }

        private sealed class BabyKustoLiteralAnyExpression : BabyKustoLiteralExpression
        {
            private readonly object _value;

            public BabyKustoLiteralAnyExpression(BabyKustoEngine engine, LiteralExpression expression)
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
