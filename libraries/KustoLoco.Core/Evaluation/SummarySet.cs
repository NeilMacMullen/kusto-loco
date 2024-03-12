using System.Collections.Generic;
using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Evaluation;

internal readonly record struct SummarySet(
    object?[] ByValues,
    List<ITableChunk> SummarisedChunks,
    List<int> RowIds);