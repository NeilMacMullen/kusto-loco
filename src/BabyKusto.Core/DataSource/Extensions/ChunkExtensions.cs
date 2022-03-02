// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BabyKusto.Core.Extensions
{
    internal static class ChunkExtensions
    {
        public static TableChunk ReParent(this ITableChunk chunk, ITableSource newOwner)
        {
            return new TableChunk(newOwner, chunk.Columns);
        }
    }
}