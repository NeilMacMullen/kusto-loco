using Kusto.Language.Symbols;
using NLog;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

/// <summary>
///     An operation which can be used to trace calls for debugging purposes
/// </summary>
internal class DebugEmitImpl : IScalarFunctionImpl
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        var left = (string?)arguments[0].Value;
        Logger.Warn(left);
        return new ScalarResult(ScalarTypes.Int, 0);
    }


    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var left = (TypedBaseColumn<string?>)(arguments[0].Column);

        var data = new int?[left.RowCount];
        for (var i = 0; i < left.RowCount; i++)
        {
            Logger.Warn(left);
            data[i] = i;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}