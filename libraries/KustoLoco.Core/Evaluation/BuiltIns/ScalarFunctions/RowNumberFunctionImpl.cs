using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

//TODO - need to check this is only called on serialized table
internal class RowNumberFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments) => new(ScalarTypes.Long, 0);

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);
        //the first column is a "dummy" column inserted by the evaluator
        //because we specified this as "ForceColumnarResult"
        var column = (TypedBaseColumn<int?>)arguments[0].Column;

        var indexResetValue = 1L;
        if (arguments.Length > 1)
        {
            var indexResetColumn = (TypedBaseColumn<long?>)arguments[1].Column;
            indexResetValue = indexResetColumn[0] ?? 0;
        }

        var resetColumn =
            arguments.Length > 2
                ? (TypedBaseColumn<bool?>)arguments[2].Column
                : (TypedBaseColumn<bool?>)
                ColumnHelpers.CreateFromScalar(false,
                    ScalarTypes.Bool, column.RowCount);

        var data = NullableSetBuilderOflong.CreateFixed(column.RowCount);

        var index = indexResetValue;
        for (var i = 0; i < column.RowCount; i++)
        {
            if (resetColumn[i] == true)
                index = indexResetValue;
            data[i] = index++;
        }

        return new ColumnarResult(GenericColumnFactoryOflong.CreateFromDataSet(data.ToNullableSet()));
    }
}
