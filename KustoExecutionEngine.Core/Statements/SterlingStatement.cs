using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Statements
{
    internal abstract class SterlingStatement<T> : SterlingStatement
        where T : Statement
    {
        private readonly T _statement;

        protected SterlingStatement(SterlingEngine engine, T statement)
            : base(engine)
        {
            _statement = statement;
        }
    }

    internal abstract class SterlingStatement
    {
        protected readonly SterlingEngine _engine;
        protected bool _initialized = true;

        internal SterlingStatement(SterlingEngine engine)
        {
            _engine = engine;
        }

        public object Execute()
        {
            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

            return ExecuteInternal();
        }

        protected virtual void Initialize()
        {
        }

        protected abstract object ExecuteInternal();

        protected internal static SterlingStatement Build(SterlingEngine engine, Statement statement)
        {
            return statement.Kind switch
            {
                SyntaxKind.ExpressionStatement => new SterlingExpressionStatement(engine, (ExpressionStatement)statement),
                _ => throw new InvalidOperationException($"Unsupported statement type '{statement.Kind}'."),
            };
        }
    }
}
