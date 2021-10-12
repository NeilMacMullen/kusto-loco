using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Operators
{
    internal abstract class StirlingOperator<T> : StirlingOperator
        where T : QueryOperator
    {
        protected readonly T _operator;

        protected StirlingOperator(StirlingEngine engine, T @operator)
            : base(engine)
        {
            _operator = @operator;
        }
    }

    internal abstract class StirlingOperator
    {
#if DEBUG
        static int DebugIndent = 0;
#endif

        protected bool _initialized = true;
        protected readonly StirlingEngine _engine;

        protected StirlingOperator(StirlingEngine engine)
        {
            _engine = engine;
        }

        public ITabularSource Evaluate(ITabularSource input)
        {
            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

#if DEBUG
            Console.WriteLine($"{new string(' ', DebugIndent)}Evaluating operator {TypeNameHelper.GetTypeDisplayName(this)}");
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

        protected abstract ITabularSource EvaluateInternal(ITabularSource input);

        internal static StirlingOperator Build(StirlingEngine engine, QueryOperator @operator)
        {
            return @operator.Kind switch
            {
                SyntaxKind.FilterOperator => new StirlingFilterOperator(engine, (FilterOperator)@operator),
                SyntaxKind.SummarizeOperator => new StirlingSummarizeOperator(engine, (SummarizeOperator)@operator),
                _ => throw new InvalidOperationException($"Unsupported operator kind '{@operator.Kind}'."),
            };
        }
    }
}
