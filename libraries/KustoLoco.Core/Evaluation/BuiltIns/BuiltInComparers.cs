﻿//
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;
using Kusto.Language.Symbols;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal static class BuiltInComparers
{
    private static readonly Dictionary<(SortDirections, NullsDirections, TypeSymbol), IComparer> comparers = new();

    static BuiltInComparers()
    {
        comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.Int), new IntAscNullsFirstComparer());
        comparers.Add((SortDirections.Asc, NullsDirections.Last, ScalarTypes.Int), new IntAscNullsLastComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.First, ScalarTypes.Int), new IntDescNullsFirstComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.Int), new IntDescNullsLastComparer());

        comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.Long), new LongAscNullsFirstComparer());
        comparers.Add((SortDirections.Asc, NullsDirections.Last, ScalarTypes.Long), new LongAscNullsLastComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.First, ScalarTypes.Long), new LongDescNullsFirstComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.Long), new LongDescNullsLastComparer());

        comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.Real), new DoubleAscNullsFirstComparer());
        comparers.Add((SortDirections.Asc, NullsDirections.Last, ScalarTypes.Real), new DoubleAscNullsLastComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.First, ScalarTypes.Real),
            new DoubleDescNullsFirstComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.Real), new DoubleDescNullsLastComparer());

        comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.Decimal), new DecimalAscNullsFirstComparer());
        comparers.Add((SortDirections.Asc, NullsDirections.Last, ScalarTypes.Decimal), new DecimalAscNullsLastComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.First, ScalarTypes.Decimal), new DecimalDescNullsFirstComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.Decimal), new DecimalDescNullsLastComparer());

        comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.Bool), new BoolAscNullsFirstComparer());
        comparers.Add((SortDirections.Asc, NullsDirections.Last, ScalarTypes.Bool), new BoolAscNullsLastComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.First, ScalarTypes.Bool), new BoolDescNullsFirstComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.Bool), new BoolDescNullsLastComparer());

        comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.String),
            new StringAscNullsFirstComparer());
        comparers.Add((SortDirections.Asc, NullsDirections.Last, ScalarTypes.String), new StringAscNullsLastComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.First, ScalarTypes.String),
            new StringDescNullsFirstComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.String),
            new StringDescNullsLastComparer());

        comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.TimeSpan),
            new TimeSpanAscNullsFirstComparer());
        comparers.Add((SortDirections.Asc, NullsDirections.Last, ScalarTypes.TimeSpan),
            new TimeSpanAscNullsLastComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.First, ScalarTypes.TimeSpan),
            new TimeSpanDescNullsFirstComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.TimeSpan),
            new TimeSpanDescNullsLastComparer());

        comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.DateTime),
            new DateTimeAscNullsFirstComparer());
        comparers.Add((SortDirections.Asc, NullsDirections.Last, ScalarTypes.DateTime),
            new DateTimeAscNullsLastComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.First, ScalarTypes.DateTime),
            new DateTimeDescNullsFirstComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.DateTime),
            new DateTimeDescNullsLastComparer());

        comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.Guid), new GuidAscNullsFirstComparer());
        comparers.Add((SortDirections.Asc, NullsDirections.Last, ScalarTypes.Guid), new GuidAscNullsLastComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.First, ScalarTypes.Guid), new GuidDescNullsFirstComparer());
        comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.Guid), new GuidDescNullsLastComparer());
    }

    public static IComparer GetComparer(SortDirections direction, NullsDirections nullsDirections, TypeSymbol type)
    {
        if (!comparers.TryGetValue((direction, nullsDirections, type), out var comparer))
        {
            throw new InvalidOperationException(
                $"Comparer not implemented for {SchemaDisplay.GetText(type)} {direction} nulls {nullsDirections}");
        }

        return comparer;
    }
}
