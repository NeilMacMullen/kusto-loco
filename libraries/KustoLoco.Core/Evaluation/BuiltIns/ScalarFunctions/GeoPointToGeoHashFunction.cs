using KustoLoco.Core.LibraryFunctions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.GeoPointToGeohash")]
public partial class GeoPointToGeoHashFunction
{
    private static string Impl(double lon, double lat, long resolution)
        => GeoSupport.GeoHash(lon, lat, resolution);

    private static string IntImpl(double lon, double lat, int resolution)
        => GeoSupport.GeoHash(lon, lat, resolution);

    private static string DefaultImpl(double lon, double lat)
        => GeoSupport.GeoHash(lon, lat, GeoPointToGeoHashFunction.DefaultResolution);

    public const int DefaultResolution = 5;
}
