using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Statements
{
    internal abstract class StirlingStatement<T> : StirlingStatement
        where T : Statement
    {
        private readonly T _statement;

        protected StirlingStatement(StirlingEngine engine, T statement)
            : base(engine)
        {
            _statement = statement;
        }
    }

    internal abstract class StirlingStatement
    {
        protected readonly StirlingEngine _engine;
        protected bool _initialized = true;

        internal StirlingStatement(StirlingEngine engine)
        {
            _engine = engine;
        }

        public object? Execute()
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

        protected abstract object? ExecuteInternal();

        protected internal static StirlingStatement Build(StirlingEngine engine, Statement statement)
        {
            return statement.Kind switch
            {
                SyntaxKind.LetStatement => new StirlingLetStatement(engine, (LetStatement)statement),
                SyntaxKind.ExpressionStatement => new StirlingExpressionStatement(engine, (ExpressionStatement)statement),
                _ => throw new InvalidOperationException($"Unsupported statement type '{statement.Kind}'."),
            };
        }
    }
}
