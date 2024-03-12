using System.Collections.Generic;

namespace KustoLoco.Core.Evaluation;

internal readonly record struct SummarySet(
    object?[] ByValues,
    List<ITableChunk> SummarisedChunks,
    List<int> RowIds);