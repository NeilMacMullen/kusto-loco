// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class GeoDistance2PointsFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 4);
        var p1Lon = (double?)arguments[0].Value;
        var p1Lat = (double?)arguments[1].Value;
        var p2Lon = (double?)arguments[2].Value;
        var p2Lat = (double?)arguments[3].Value;
        return new ScalarResult(ScalarTypes.Real, Compute(p1Lon, p1Lat, p2Lon, p2Lat));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 4);
        var p1LonColumn = (Column<double?>)arguments[0].Column;
        var p1LatColumn = (Column<double?>)arguments[1].Column;
        var p2LonColumn = (Column<double?>)arguments[2].Column;
        var p2LatColumn = (Column<double?>)arguments[3].Column;
        Debug.Assert(p1LonColumn.RowCount == p1LatColumn.RowCount && p1LonColumn.RowCount == p2LonColumn.RowCount &&
                     p1LonColumn.RowCount == p2LatColumn.RowCount);

        var data = new double?[p1LonColumn.RowCount];
        for (var i = 0; i < p1LonColumn.RowCount; i++)
        {
            data[i] = Compute(p1LonColumn[i], p1LatColumn[i], p2LonColumn[i], p2LatColumn[i]);
        }

        return new ColumnarResult(Column.Create(ScalarTypes.Real, data));
    }

    private static double? Compute(double? lon1, double? lat1, double? lon2, double? lat2)
    {
        if (lon1.HasValue && lon1.Value >= -180 && lon1.Value <= 180 &&
            lat1.HasValue && lat1.Value >= -90 && lat1.Value <= 90 &&
            lon2.HasValue && lon2.Value >= -180 && lon2.Value <= 180 &&
            lat2.HasValue && lat2.Value >= -90 && lat2.Value <= 90)
        {
            // Haversine formula....
            // Real Kusto implements WGS-84 calculation, but we take the easy approach and assume the Earth is a perfect sphere...
            var lat1rad = lat1.Value * Math.PI / 180;
            var lat2rad = lat2.Value * Math.PI / 180;
            var sinDeltaLat = Math.Sin((lat2rad - lat1rad) / 2);
            var sinDeltaLon = Math.Sin(((lon2.Value - lon1.Value) * Math.PI / 180) / 2);
            var a = (sinDeltaLat * sinDeltaLat) + Math.Cos(lat1rad) * Math.Cos(lat2rad) * (sinDeltaLon * sinDeltaLon);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            const double R = 6371e3;
            var d = R * c;
            return d;
        }

        return null;
    }
}