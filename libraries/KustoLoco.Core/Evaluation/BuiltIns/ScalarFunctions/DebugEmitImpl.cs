using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

/// <summary>
///     An operation which can be used to trace calls for debugging purposes
/// </summary>
internal class DebugEmitImpl : IScalarFunctionImpl
{
    

    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        var left = (string?)arguments[0].Value;
        //Logger.Warn(left ?? "<null>");
        return new ScalarResult(ScalarTypes.Int, 0);
    }


    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var left = (GenericTypedBaseColumnOfstring)(arguments[0].Column);

        var data = NullableSetBuilderOfint.CreateFixed(left.RowCount);
        for (var i = 0; i < left.RowCount; i++)
        {
            //Logger.Warn(left);
            data[i] = i;
        }

        return new ColumnarResult(GenericColumnFactoryOfint
            .CreateFromDataSet(data.ToNullableSet()));
    }
}
