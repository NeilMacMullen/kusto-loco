namespace LokqlDx.ViewModels;

public class GeoPoint(double lat, double lon, bool valid)
{
    public readonly double Latitude = lat;
    public readonly double Longitude = lon;
    public readonly bool Valid = valid;

    public GeoPoint(double latitude, double longitude) : this(latitude, longitude, true)
    {
    }

    public static GeoPoint Maybe(object? lat, object? lon)
    {
        if (lat is not double dlat || lon is not double dlon)
            return new GeoPoint(0, 0, false);
        return new GeoPoint(dlat, dlon, true);
    }
}
