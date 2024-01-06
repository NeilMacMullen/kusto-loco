using System.Diagnostics;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

//TODO - this probably needs to be a diffent kind of function
//the kusto docs say it can be invoked without any arguments
//in which case where woudl we get the column height
//Even with the column height, we're snookered with multiple
//chunks unless we force a full materialization
//And when called with constant parameters as suggested in the examples,
//we get invoked in scalar context even though we really need to be
//returning a column
//The kusto docs suggest this can only be called on a _serialized_ table so
//we probably need to extend the schema to detected when we're dealing with chunks
internal class RowNumberFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments) => new(ScalarTypes.Long, 0);

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);
        //the first column is a "dummy" column inserted by the evaluator
        //because we specified this as "ForceColumnarResult"
        var column = (TypedBaseColumn<int?>)arguments[0].Column;

        var indexResetValue = 0L;
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

        var data = new long?[column.RowCount];

        var index = indexResetValue;
        for (var i = 0; i < column.RowCount; i++)
        {
            if (resetColumn[i] == true)
                index = indexResetValue;
            data[i] = index++;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}