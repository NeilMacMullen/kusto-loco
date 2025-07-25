using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class ArgMaxFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<string?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var set = new HashSet<string>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            var v = valuesColumn[i];
            if (!string.IsNullOrEmpty(v))
            {
                set.Add(v);
                if (set.Count >= maxSize)
                {
                    break;
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}
