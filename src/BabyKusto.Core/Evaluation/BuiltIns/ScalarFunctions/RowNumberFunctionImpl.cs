using System.Diagnostics;
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
        var column = (TypedBaseColumn<long?>)arguments[0].Column;
        var data = new long?[column.RowCount];
        var index = 0;
        for (var i = 0; i < column.RowCount; i++)
        {
            data[i] = index++;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}