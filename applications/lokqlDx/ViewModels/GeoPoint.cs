using Mapsui.Extensions;

namespace LokqlDx.ViewModels;

public class GeoPoint(double lat, double lon, bool valid)
{
    public readonly double Latitude = lat;
    public readonly double Longitude = lon;
    public readonly bool Valid = valid;

    public GeoPoint(double latitude, double longitude) : this(latitude, longitude, true)
    {
    }

    public static GeoPoint Maybe(double lat, double lon)
    {
        if (double.IsFinite(lat) && double.IsFinite(lon))
            return new GeoPoint(lat, lon, true);
     
        return new GeoPoint(0, 0, false);
    }
}
