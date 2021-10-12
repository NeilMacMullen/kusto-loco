using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal abstract class SterlingExpression
    {
        protected bool _initialized = true;

        protected readonly SterlingEngine _engine;
        protected internal readonly Expression _expression;

        public SterlingExpression(SterlingEngine engine, Expression expression)
        {
            _engine = engine;
            _expression = expression;
        }

        public object Evaluate()
        {
            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

            return EvaluateInternal();
        }

        protected virtual void Initialize()
        {
        }

        protected abstract object EvaluateInternal();

        protected internal static SterlingExpression Build(SterlingEngine engine, Expression expression)
        {
            return expression.Kind switch
            {
                SyntaxKind.NameReference => new SterlingNameReferenceExpression(engine, (NameReference)expression),
                SyntaxKind.PipeExpression => new SterlingPipeExpression(engine, (PipeExpression)expression),
                SyntaxKind.FilterOperator or SyntaxKind.SummarizeOperator => SterlingOperatorExpression.Build(engine, (FilterOperator)expression),
                SyntaxKind.AddExpression or SyntaxKind.SubtractExpression => SterlingBinaryExpression.Build(engine, (BinaryExpression)expression),
                _ => throw new InvalidOperationException($"Unsupported expression kind '{expression.Kind}'."),
            };
        }
    }
}
