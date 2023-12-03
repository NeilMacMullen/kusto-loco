using System.Collections.Generic;

namespace BabyKusto.Core.Evaluation;

internal partial class TreeEvaluator
{
   
    private readonly record struct NpmSummarySet(object?[] ByValues,
        List<ITableChunk> SummarisedChunks,
        List<int> RowIds);
}