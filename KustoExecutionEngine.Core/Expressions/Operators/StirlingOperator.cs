using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.DataSource;

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
            return EvaluateInternal((ITabularSourceV2)input!);
        }

        protected abstract object EvaluateInternal(ITabularSourceV2 input);
    }
}
