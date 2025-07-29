﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.LibraryFunctions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class GeoDistance2PointsFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 4);
        var p1Lon = (double?)arguments[0].Value;
        var p1Lat = (double?)arguments[1].Value;
        var p2Lon = (double?)arguments[2].Value;
        var p2Lat = (double?)arguments[3].Value;
        return new ScalarResult(ScalarTypes.Real, GeoSupport.HaversineDistance(p1Lon, p1Lat, p2Lon, p2Lat));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 4);
        var p1LonColumn = (TypedBaseColumn<double?>)arguments[0].Column;
        var p1LatColumn = (TypedBaseColumn<double?>)arguments[1].Column;
        var p2LonColumn = (TypedBaseColumn<double?>)arguments[2].Column;
        var p2LatColumn = (TypedBaseColumn<double?>)arguments[3].Column;
        Debug.Assert(p1LonColumn.RowCount == p1LatColumn.RowCount && p1LonColumn.RowCount == p2LonColumn.RowCount &&
                     p1LonColumn.RowCount == p2LatColumn.RowCount);

        var data = NullableSetBuilderOfdouble.CreateFixed(p1LonColumn.RowCount);

        var blockSize = 1000;
        var rangePartitioner = Partitioner.Create(0, p1LonColumn.RowCount, blockSize);
        Parallel.ForEach(rangePartitioner, (range, loopState) =>
        {
            for (var i = range.Item1; i < range.Item2; i++)
            {
                data[i] = GeoSupport.HaversineDistance(p1LonColumn[i], p1LatColumn[i], p2LonColumn[i], p2LatColumn[i]);
            }
        });
        return new ColumnarResult(ColumnFactory.CreateFromDataSet(data.ToNullableSet()));
    }
}
