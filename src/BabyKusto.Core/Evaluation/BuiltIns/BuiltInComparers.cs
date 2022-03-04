// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using BabyKusto.Core.Evaluation.BuiltIns.Impl;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns
{
    internal static class BuiltInComparers
    {
        private static Dictionary<(SortDirections, NullsDirections, TypeSymbol), IComparer> comparers = new();

        static BuiltInComparers()
        {
            comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.Int), new IntAscComparer());
            comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.Int), new IntDescComparer());

            comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.Long), new LongAscComparer());
            comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.Long), new LongDescComparer());

            comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.Real), new DoubleAscComparer());
            comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.Real), new DoubleDescComparer());

            comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.Bool), new BoolAscComparer());
            comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.Bool), new BoolDescComparer());

            comparers.Add((SortDirections.Asc, NullsDirections.First, ScalarTypes.String), new StringAscComparer());
            comparers.Add((SortDirections.Desc, NullsDirections.Last, ScalarTypes.String), new StringDescComparer());
        }

        public static IComparer GetComparer(SortDirections direction, NullsDirections nullsDirections, TypeSymbol type)
        {
            if (!comparers.TryGetValue((direction, nullsDirections, type), out var comparer))
            {
                throw new InvalidOperationException($"Comparer not implemented for {type.Display} {direction} nulls {nullsDirections}");
            }

            return comparer;
        }
    }
}
