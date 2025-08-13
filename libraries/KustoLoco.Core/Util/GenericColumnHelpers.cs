//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Nodes;
using KustoLoco.Core.Extensions;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Util;

public static class GenericColumnHelpers
{
  
    private static GenericTypedBaseColumn<T> Create<T>(ImmutableArray<int> mapping, GenericTypedBaseColumn<T>[] others,
        MappingType mapType)
    {
        return mapType switch
               {
                   MappingType.Arbitrary => MapColumn<T>(others[0], mapping),
                   MappingType.Chunk => GenericChunkColumn<T>.Create(mapping[0], mapping[1], others[0]),
                   MappingType.Reassembly =>  GenericReassembledChunkColumn<T>.Create(others), 
                   _ => throw new NotImplementedException()
               };
    }

    private static GenericTypedBaseColumn<T> MapColumn<T>(IEnumerable<BaseColumn> others, ImmutableArray<int> mapping,
        MappingType mapType)
    {
        return Create(mapping, others.Cast<GenericTypedBaseColumn<T>>().ToArray(), mapType);
       
    }


    public static GenericTypedBaseColumn<T> MapColumn<T>(GenericTypedBaseColumn<T> others, ImmutableArray<int> mapping)
        => GenericMappedColumn<T>.Create(mapping, others);

    private static GenericTypedBaseColumn<T> CreateFromObjectArray<T>(object?[] data)
    {

        return new GenericInMemoryColumn<T>(data);
    }


  
    private enum MappingType
    {
        Arbitrary,
        Chunk,
        Reassembly
    }
}
