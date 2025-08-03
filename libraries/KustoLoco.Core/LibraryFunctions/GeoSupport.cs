﻿using System;
using Geohash;

namespace KustoLoco.Core.LibraryFunctions;

public static class GeoSupport
{
    private static bool CheckValid(double? lon1, double? lat1)
        => lon1 is >= -180 and <= 180 &&
           lat1 is >= -90 and <= 90;

    private static double ToRadians(double degrees)
        => degrees * Math.PI / 180;

    private static double ToRadians(double? degrees)
        => ToRadians(degrees!.Value);

    public static double? HaversineDistance(double? lon1, double? lat1, double? lon2, double? lat2)
    {
        if (!CheckValid(lon1, lat1))
            return null;
        if (!CheckValid(lon2, lat2)) return null;

        // Haversine formula....
        // Real Kusto implements WGS-84 calculation, but we take the easy approach and assume the Earth is a perfect sphere...
        var lat1rad = ToRadians(lat1);
        var lat2rad = ToRadians(lat2);
        var sinDeltaLat = Math.Sin((lat2rad - lat1rad) / 2);
        var sinDeltaLon = Math.Sin(ToRadians(lon2 - lon1) / 2);
        var a = (sinDeltaLat * sinDeltaLat) +
                Math.Cos(lat1rad) * Math.Cos(lat2rad) * (sinDeltaLon * sinDeltaLon);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        const double R = 6371e3;
        var d = R * c;
        return d;
    }

    public static string GeoHash(double lon, double lat, long resolution)
    {
        if (!CheckValid(lon, lat))
            return string.Empty;

        var geohasher = new Geohasher();

        // Encode latitude and longitude into a geohash string
        return geohasher.Encode(lat, lon, (int)resolution);
    }

    public static (double Latitude, double Longitude) GeoHashCentralPoint(string point)
    {
        var geohasher = new Geohasher();
        var d = geohasher.Decode(point);
        return (d.latitude, d.longitude);
    }
}
