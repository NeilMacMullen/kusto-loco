using System.Collections.Generic;
using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation;

internal partial class TreeEvaluator
{
    private readonly record struct NpmSummarySet(object?[] ByValues,
        IndirectColumnBuilder[] IndirectionBuilders,
        List<int> SelectedRows);
}