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

        /// <summary>
        /// Evaluates the expression.
        /// <paramref name="input"/> can be null, a scalar or an <see cref="ITabularSource"/>.
        /// </summary>
        public object Evaluate(object? input)
        {
            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

#if DEBUG
            Console.WriteLine($"{new string(' ', DebugIndent)}Evaluating expression {TypeNameHelper.GetTypeDisplayName(this)} ({_expression.ToString(IncludeTrivia.SingleLine)})");
            DebugIndent++;
            try
            {
#endif
                return EvaluateInternal(input);
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

        protected abstract object EvaluateInternal(object? input);

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

                SyntaxKind.BooleanLiteralExpression or
                SyntaxKind.IntLiteralExpression or
                SyntaxKind.LongLiteralExpression or
                SyntaxKind.RealLiteralExpression or
                SyntaxKind.DecimalLiteralExpression or
                SyntaxKind.DateTimeLiteralExpression or
                SyntaxKind.TimespanLiteralExpression or
                SyntaxKind.GuidLiteralExpression or
                SyntaxKind.StringLiteralExpression or
                SyntaxKind.NullLiteralExpression => StirlingLiteralExpression.Build(engine, (LiteralExpression)expression),

                SyntaxKind.ParenthesizedExpression => new StirlingParenthesizedExpression(engine, (ParenthesizedExpression)expression),

                SyntaxKind.DataTableExpression => new StirlingDataTableExpressions(engine, (DataTableExpression)expression),

                _ => throw new InvalidOperationException($"Unsupported expression kind '{expression.Kind}'."),
            };
        }
    }
}
