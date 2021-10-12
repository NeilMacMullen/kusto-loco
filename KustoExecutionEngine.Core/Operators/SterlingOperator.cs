using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Operators
{
    internal abstract partial class SterlingOperator
    {
#if DEBUG
        static int DebugIndent = 0;
#endif

        protected bool _initialized = true;
        protected readonly SterlingEngine _engine;
        protected readonly QueryOperator _operator;

        protected SterlingOperator(SterlingEngine engine, QueryOperator @operator)
        {
            _engine = engine;
            _operator = @operator;
        }

        public ITabularSource Evaluate(ITabularSource input)
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

        internal static SterlingOperator Build(SterlingEngine engine, QueryOperator @operator)
        {
            return @operator.Kind switch
            {
                SyntaxKind.FilterOperator => new SterlingFilterOperator(engine, (FilterOperator)@operator),
                SyntaxKind.SummarizeOperator => new SterlingSummarizeOperator(engine, (SummarizeOperator)@operator),
                _ => throw new InvalidOperationException($"Unsupported operator kind '{@operator.Kind}'."),
            };
        }
    }
}
