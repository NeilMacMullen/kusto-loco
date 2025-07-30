using System.Collections.Concurrent;
using System.Threading.Tasks;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.LibraryFunctions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class GeoPointToGeoHashFunctionImpl : IScalarFunctionImpl
{
    private const long DefaultResolution = 5;

    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        var p1Lon = (double?)arguments[0].Value;
        var p1Lat = (double?)arguments[1].Value;
        var resolution = (arguments.Length == 3)
            ? (long?)arguments[2].Value
            : DefaultResolution;
        return new ScalarResult(ScalarTypes.String, GeoSupport.GeoHash(p1Lon, p1Lat, resolution));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var p1LonColumn = (GenericTypedBaseColumnOfdouble)arguments[0].Column;
        var p1LatColumn = (GenericTypedBaseColumnOfdouble)arguments[1].Column;

        var rowCount = p1LonColumn.RowCount;
        var resColumn = arguments.Length > 2
            ? (GenericTypedBaseColumnOflong)arguments[2].Column
            : new GenericSingleValueColumnOflong(DefaultResolution, rowCount);

        var data = NullableSetBuilderOfstring.CreateFixed(rowCount);

        var blockSize = 1000;
        var rangePartitioner = Partitioner.Create(0, p1LonColumn.RowCount, blockSize);
        Parallel.ForEach(rangePartitioner, (range, loopState) =>
        {
            for (var i = range.Item1; i < range.Item2; i++)
            {
                data[i] = GeoSupport.GeoHash(p1LonColumn[i], p1LatColumn[i], resColumn[i]);
            }
        });
        return new ColumnarResult(ColumnFactory.CreateFromDataSet(data.ToNullableSet()));
    }
}
