using System.Text.Json.Nodes;
using KustoLoco.Core.LibraryFunctions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.GeohashToCentralPoint")]
public partial class GeoHashToCentralPointFunction
{
    public JsonNode HImpl(string hash)
    {
        var pt = GeoSupport.GeoHashCentralPoint(hash);
        return GeoHashToCentralPointFunction.PtToJson(pt.Latitude, pt.Longitude);
    }


    internal static JsonObject PtToJson(double latitude, double longitude) =>
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
