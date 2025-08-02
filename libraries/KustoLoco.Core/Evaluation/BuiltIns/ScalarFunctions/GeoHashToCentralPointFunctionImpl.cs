using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.LibraryFunctions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class GeoHashToCentralPointFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        var p1Lon = (string?)arguments[0].Value;

        var pt = GeoSupport.GeoHashCentralPoint(p1Lon!);


        var j = PtToJson(pt.Latitude, pt.Longitude);

        return new ScalarResult(ScalarTypes.Dynamic, j);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var hash = (GenericTypedBaseColumnOfstring)arguments[0].Column;

        var rowCount = hash.RowCount;

        var data = NullableSetBuilderOfJsonNode.CreateFixed(rowCount);

        var rangePartitioner = SafePartitioner.Create(rowCount);
        Parallel.ForEach(rangePartitioner, (range, loopState) =>
        {
            for (var i = range.Item1; i < range.Item2; i++)
            {
                var pt = GeoSupport.GeoHashCentralPoint(hash[i]!);
                data[i] = PtToJson(pt.Latitude, pt.Longitude);
            }
        });
        return new ColumnarResult(GenericColumnFactoryOfJsonNode.CreateFromDataSet(data.ToNullableSet()));
    }

    private static JsonObject PtToJson(double latitude, double longitude) =>
        new()
        {
            ["type"] = "Point",
            ["coordinates"] = new JsonArray
            {
                longitude,
                latitude
            }
        };
}
