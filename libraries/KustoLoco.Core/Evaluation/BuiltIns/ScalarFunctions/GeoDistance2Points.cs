// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.LibraryFunctions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.GeoDistance2Points")]
public partial class GeoDistance2PointsFunction
{
    public double? Impl(double p1Lon, double p1Lat, double p2Lon, double p2Lat)
        => GeoSupport.HaversineDistance(p1Lon, p1Lat, p2Lon, p2Lat);
}
