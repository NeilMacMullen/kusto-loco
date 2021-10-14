using System.Collections.Generic;
using KustoExecutionEngine.Core.DataSource;

namespace KustoExecutionEngine.Core
{
    public interface ITabularSource
    {
        IRow? GetNextRow();
    }

    public interface IRow
    {
        TableSchema Schema { get; }

        object?[] Values { get; }
    }
}
