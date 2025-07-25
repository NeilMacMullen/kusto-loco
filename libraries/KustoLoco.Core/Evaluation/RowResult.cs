using System.Collections.Generic;
using System.Linq;

namespace KustoLoco.Core.Evaluation;

public class RowResult :EvaluationResult
{
    public  RowResult(IEnumerable<object?> values) : base(NullTypeSymbol.Instance)
    {
        Values = values.ToArray();
    }

    public readonly object?[] Values;
}
