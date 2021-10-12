using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions
{
    internal abstract class StirlingExpression
    {
#if DEBUG
        static int DebugIndent = 0;
#endif

        protected bool _initialized = true;

        protected readonly StirlingEngine _engine;
        protected internal readonly Expression _expression;

        public StirlingExpression(StirlingEngine engine, Expression expression)
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

#if DEBUG
            Console.WriteLine($"{new string(' ', DebugIndent)}Evaluating expression {TypeNameHelper.GetTypeDisplayName(this)}");
            DebugIndent++;
            try
            {
#endif
                return EvaluateInternal();
#if DEBUG
            }
            finally
            {
                DebugIndent--;
            }
#endif
        }

        protected virtual void Initialize()
        {
        }

        protected abstract object EvaluateInternal();

        protected internal static StirlingExpression Build(StirlingEngine engine, Expression expression)
        {
            return expression.Kind switch
            {
                SyntaxKind.NameReference => new StirlingNameReferenceExpression(engine, (NameReference)expression),
                SyntaxKind.PipeExpression => new StirlingPipeExpression(engine, (PipeExpression)expression),
                SyntaxKind.SimpleNamedExpression => new StirlingSimpleNamedExpression(engine, (SimpleNamedExpression)expression),

                SyntaxKind.AddExpression or
                SyntaxKind.SubtractExpression or
                SyntaxKind.MultiplyExpression or
                SyntaxKind.DivideExpression => StirlingBinaryExpression.Build(engine, (BinaryExpression)expression),

                _ => throw new InvalidOperationException($"Unsupported expression kind '{expression.Kind}'."),
            };
        }
    }
}
