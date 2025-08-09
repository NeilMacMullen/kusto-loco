using KustoLoco.Core.LibraryFunctions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.GeoPointToGeohash")]
public partial class GeoPointToGeoHashFunction
{
    public string Impl(double lon, double lat, long resolution)
    {
        return GeoSupport.GeoHash(lon, lat, resolution);
    }
    public string IntImpl(double lon, double lat, int resolution)
    {
        return GeoSupport.GeoHash(lon, lat, resolution);
    }


    public string DefaultResolutionImpl(double lon, double lat)
    {
        return GeoSupport.GeoHash(lon, lat, GeoPointToGeoHashFunction.DefaultResolution);
    }

    public const int DefaultResolution = 5;
}
