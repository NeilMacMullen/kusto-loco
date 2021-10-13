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



    // TODO Tommy:
    public interface ITabularSourceV2
    {
        ITableSchema Schema { get; }

        ITableChunk GetNextChunk();
    }

    public interface ITableChunk
    {
        List<List<KustoValue>> ColumnValues { get; }
    }

    public interface ITableSchema
    {
        List<ColumnDefinition> ColumnDefinitions { get; }

        int GetColumnIndex(string columnName);
    }

    public interface ColumnDefinition
    {
        string ColumnName { get; }
        KustoValueKind ValueType { get; }
    }

    public struct KustoValue
    {
        KustoValueKind ValueType { get; }
        object Value { get; }
    }

    public enum KustoValueKind
    {
        Bool,

        Int,

        Long,

        Real,

        Decimal,

        String,

        DateTime,

        TimeSpan,

        Guid,

        Type,

        Dynamic
    }
}
