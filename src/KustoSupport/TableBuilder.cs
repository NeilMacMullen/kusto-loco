using System.Collections;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Text.Json;
using BabyKusto.Core;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;
using NLog;

namespace KustoSupport;

#pragma warning disable CS8604, CS8602, CS8603
/// <summary>
///     Provides a simple way to create a Kusto ITableSource
/// </summary>
/// <remarks>
///     TODO This is currently a bit incomplete - more type support is needed
/// </remarks>
public class TableBuilder
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public readonly int Length;
    public readonly string Name;

    private ImmutableArray<string> _columnNames = ImmutableArray<string>.Empty;

    private ImmutableArray<BaseColumn> _columns = ImmutableArray<BaseColumn>.Empty;

    private TableBuilder(string name, IEnumerable<BaseColumn> columns,
        IEnumerable<string> columnNames, int length)
    {
        _columns = columns.ToImmutableArray();
        _columnNames = columnNames.ToImmutableArray();
        Length = length;
        Name = name;
    }

    public static TableBuilder CreateEmpty(string name, int length)
        => new(
               name,
               Array.Empty<BaseColumn>(),
               Array.Empty<string>(),
               length);


    public TableBuilder WithColumn(string name, BaseColumn column)
    {
        _columns = _columns.Add(column);
        _columnNames = _columnNames.Add(name);
        return this;
    }

    public TableBuilder WithColumn<T>(string name, IEnumerable<T> items)
    {
        var column = ColumnFactory.Create(items.ToArray());
        return WithColumn(name, column);
    }

    public TableBuilder WithColumn(string name, Type type, IReadOnlyCollection<object?> items)
    {
        var builder = ColumnHelpers.CreateBuilder(type);
        foreach (var item in items)
        {
            builder.Add(item);
        }

        return WithColumn(name, builder.ToColumn());
    }

    public TableBuilder WithIndexColumn<T>(string name, T? value)
    {
        var indexColumn = new SingleValueColumn<T>(value, Length);
        return WithColumn(name, indexColumn);
    }


    public static TableBuilder CreateFromRows<T>(string name, IReadOnlyCollection<T> rows) => FromRecords(name, rows);

    private static TableBuilder FromRecords<T>(string tableName, IReadOnlyCollection<T> records)
    {
        var builder = CreateEmpty(tableName, records.Count);
        foreach (var p in typeof(T).GetProperties())
        {
            var data = records.Select(r => p.GetValue(r)).ToArray();

            builder.WithColumn(p.Name, p.PropertyType, data);
        }

        return builder;
    }

    public static TableBuilder FromOrderedDictionarySet(string tableName,
        IReadOnlyCollection<OrderedDictionary> dictionaries)
    {
        //currently we rely on the dictionary having valid types and avoiding null values in unfortunate places
        var firstDictionary = dictionaries.First();
        var headers = firstDictionary.Cast<DictionaryEntry>()
                                     .Select(de => de.Key.ToString())
                                     .Select(h => new { ColumnName = h, Type = firstDictionary[h]?.GetType()??typeof(string) })
                                     .ToArray();

        var builder = CreateEmpty(tableName, dictionaries.Count);
        foreach (var header in headers)
        {
            if (header.Type == typeof(JsonElement))
            {
                Logger.Warn($"IGNORING COLUMN {header.ColumnName} because it seems to be structured data ");
                continue;
            }
            var data = dictionaries.Select(d => d[header.ColumnName]).ToArray();
            builder.WithColumn(header.ColumnName, header.Type, data);
        }

        return builder;
    }

    public static ITableSource FromScalarResult(ScalarResult scalar)
    {
        var column = ColumnHelpers.CreateFromScalar(scalar.Value, scalar.Type, 1);
        return CreateEmpty("result", 1)
               .WithColumn("value", column)
               .ToTableSource();
    }

    public ITableSource ToTableSource()
    {
        var syms = _columnNames.Zip(_columns)
                               .Select(cs =>
                                           new ColumnSymbol(cs.First, cs.Second.Type))
                               .ToArray();
        var ts = new TableSymbol(Name, syms);
        return new InMemoryTableSource(ts, _columns.ToArray());
    }

    public static ITableSource FromTable(ITableSource table, string requestedTableName)
    {
        if (table is not InMemoryTableSource ims)
            throw new NotImplementedException("can currently only shared generated tables");
        return ims.ShareAs(requestedTableName);
    }
}
