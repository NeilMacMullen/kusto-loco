using System;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Evaluation.BuiltIns;

internal class NewGuidFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments) => new(ScalarTypes.Guid, Guid.NewGuid());

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length > 0);
        //the first column is a "dummy" column inserted by the evaluator
        //because we specified this as "ForceColumnarResult"
        var column = (GenericTypedBaseColumnOfint)arguments[0].Column;

        var data = NullableSetBuilderOfGuid.CreateFixed(column.RowCount);
        for (var i = 0; i < column.RowCount; i++) data[i] = Guid.NewGuid();
        return new ColumnarResult(GenericColumnFactoryOfGuid.CreateFromDataSet(data.ToNullableSet()));
    }
}
