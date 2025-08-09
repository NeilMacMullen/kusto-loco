using System.Diagnostics;
using System.Linq;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class StrcatDelimFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {

        var j = string.Join(arguments[0].Value as string,
            arguments.Skip(1).Select(a => a.Value));
        return new ScalarResult(ScalarTypes.String, j);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length > 0);
        var columns =
            arguments.Select(c => (GenericTypedBaseColumnOfstring)c.Column)
                .ToArray();
        var delimiterColumn = columns[0];
        var stringColumns = columns.Skip(1).ToArray();

        var rowCount = columns[0].RowCount;
        var data = NullableSetBuilderOfstring.CreateFixed(rowCount);
        for (var row = 0; row < rowCount; row++)
        {
            var delimiter = delimiterColumn[row];
            var strings = stringColumns.Select(c => c[row]).ToArray();
            var joinedString = string.Join(delimiter, strings);
            data[row] = joinedString;
        }

        return new ColumnarResult(GenericColumnFactoryOfstring.CreateFromDataSet(data.ToNullableSet()));
    }
}
