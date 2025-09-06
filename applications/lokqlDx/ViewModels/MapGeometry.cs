using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;
using NetTopologySuite.Geometries;

namespace LokqlDx.ViewModels;

public static class MapGeometry
{
    public static MPoint FromGeoPoint(GeoPoint point) =>
        SphericalMercator.FromLonLat(point.Longitude, point.Latitude)
            .ToMPoint();

    public static Coordinate CoordFromGeopoint(GeoPoint geo)
    {
        var merc = SphericalMercator.FromLonLat(geo.Longitude, geo.Latitude);
        return new Coordinate(merc.x, merc.y);
    }
}
