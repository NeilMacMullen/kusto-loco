using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.Expressions;

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

    internal static class StirlingOperator
    {
        internal static StirlingExpression Build(StirlingEngine engine, QueryOperator @operator)
        {
            return @operator.Kind switch
            {
                SyntaxKind.FilterOperator => new StirlingFilterOperator(engine, (FilterOperator)@operator),
                SyntaxKind.SummarizeOperator => new StirlingSummarizeOperator(engine, (SummarizeOperator)@operator),
                SyntaxKind.ProjectOperator => new StirlingProjectOperator(engine, (ProjectOperator)@operator),
                SyntaxKind.JoinOperator => new StirlingJoinOperator(engine, (JoinOperator)@operator),
                _ => throw new InvalidOperationException($"Unsupported operator kind '{@operator.Kind}'."),
            };
        }
    }
}
