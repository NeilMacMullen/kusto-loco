using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal abstract class StirlingOperator<T> : StirlingExpression
        where T : QueryOperator
    {
        protected StirlingOperator(StirlingEngine engine, T @operator)
            : base(engine, @operator)
        {
        }

        protected sealed override object EvaluateInternal(object? input)
        {
            return EvaluateInternal((ITabularSource)input!);
        }

        protected abstract object EvaluateInternal(ITabularSource input);
    }
}
