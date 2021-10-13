using System.Collections.Generic;

namespace KustoExecutionEngine.Core
{
    public interface ITabularSource
    {
        IRow? GetNextRow();
    }

    public interface IRow : IEnumerable<KeyValuePair<string, object?>>
    {
        object? this[string columnName] { get; }
    }
}
