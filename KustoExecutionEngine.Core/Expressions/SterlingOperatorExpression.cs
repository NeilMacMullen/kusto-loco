using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal abstract class SterlingOperatorExpression : SterlingExpression
    {
        private SterlingOperatorExpression(SterlingEngine engine, QueryOperator expression)
            : base(engine, expression)
        {
        }

        internal static SterlingOperatorExpression Build(SterlingEngine engine, QueryOperator expression)
        {
            return expression.Kind switch
            {
                SyntaxKind.FilterOperator => new SterlingFilterOperatorExpression(engine, (FilterOperator)expression),
                SyntaxKind.SummarizeOperator => new SterlingSummarizeOperatorExpression(engine, (SummarizeOperator)expression),
                _ => throw new InvalidOperationException($"Unsupported operator kind '{expression.Kind}'."),
            };
        }

        private sealed class SterlingFilterOperatorExpression : SterlingOperatorExpression
        {
            public SterlingFilterOperatorExpression(SterlingEngine engine, FilterOperator expression)
                : base(engine, expression)
            {
            }

            protected override object EvaluateInternal()
            {
                // TODO: Implement filter
                return new EmptyTabularSource();
            }
        }

        private sealed class SterlingSummarizeOperatorExpression : SterlingOperatorExpression
        {
            public SterlingSummarizeOperatorExpression(SterlingEngine engine, SummarizeOperator expression)
                : base(engine, expression)
            {
            }

            protected override object EvaluateInternal()
            {
                // TODO: Implement summarize
                return new EmptyTabularSource();
            }
        }
    }
}
