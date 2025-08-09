using System;
using System.Collections.Concurrent;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

/// <summary>
///     Creates partitions that are safe to use with BitArray
/// </summary>
/// <remarks>
///     BitArray appears to use int under the hood, but we assume 64 bits
///     for paranoia
/// </remarks>
public static class SafePartitioner
{
    private const int Granularity = 64;
    private const int DefaultBlockSize = 1024*10;

    public static OrderablePartitioner<Tuple<int, int>>
        Create(int total)
        => Create(total, DefaultBlockSize);

    public static OrderablePartitioner<Tuple<int, int>>
        Create(int total, int blockSize) => Create(0, total, blockSize);

    public static OrderablePartitioner<Tuple<int, int>>
        Create(int start, int total, int blockSize)
    {
        blockSize = blockSize - (blockSize % Granularity);
        return Partitioner.Create(start, total, blockSize);
    }
}
