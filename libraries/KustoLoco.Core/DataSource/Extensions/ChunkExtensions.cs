// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Extensions;

internal static class ChunkExtensions
{
    public static TableChunk ReParent(this ITableChunk chunk, ITableSource newOwner) =>
        new(newOwner, chunk.Columns);
}