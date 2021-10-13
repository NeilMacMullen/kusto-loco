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
    }
}
