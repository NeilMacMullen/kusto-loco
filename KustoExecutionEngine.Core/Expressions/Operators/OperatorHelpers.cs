using KustoExecutionEngine.Core.DataSource;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal static class OperatorHelpers
    {
        internal static Column ProjectColumn(StirlingEngine engine, ITableChunk chunk, StirlingExpression expression)
        {
            var result = new Column(chunk.Columns[0].Size);
            for (int i = 0; i < chunk.Columns[0].Size; i++)
            {
                engine.PushRowContext(chunk.GetRow(i));
                try
                {
                    var value = expression.Evaluate(null);

                    result[i] = value;
                }
                finally
                {
                    engine.LeaveExecutionContext();
                }
            }

            return result;
        }
    }
}
