//
// Licensed under the MIT License.

using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal interface IAggregateImpl
{
    EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments);

}
