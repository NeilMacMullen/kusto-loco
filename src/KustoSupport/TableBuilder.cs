using System.Collections;
using System.Collections.Immutable;
using System.Collections.Specialized;
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

    public static TableBuilder CreateEmpty(string name, int length) =>
        new(
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

    public TableBuilder WithIndexColumn<T>(string name, T? value)
    {
        var indexColumn = new SingleValueColumn<T>(value, Length);
        return WithColumn(name, indexColumn);
    }


    public static TableBuilder CreateFromRows<T>(string name, IReadOnlyCollection<T> rows) => FromRecords(name, rows);

    private static TableBuilder FromRecords<T>(string tableName, IReadOnlyCollection<T> records)
    {
        var columnDefinitions = typeof(T).GetProperties()
            .Select(p => p.PropertyType switch
            {
                _ when p.PropertyType == typeof(int) =>
                    new KustoColumnDefinition<T>(p.Name, ScalarTypes.Int,
                        x => p.GetValue(x)),
                _ when p.PropertyType == typeof(DateTime) =>
                    new KustoColumnDefinition<T>(p.Name, ScalarTypes.DateTime,
                        x => p.GetValue(x)),
                _ when p.PropertyType == typeof(double) =>
                    new KustoColumnDefinition<T>(p.Name, ScalarTypes.Real,
                        x => p.GetValue(x)),
                _ => new KustoColumnDefinition<T>(p.Name, ScalarTypes.String,
                    x => p.GetValue(x).ToString()),
            })
            .ToList();

        return FromRows(tableName, records, columnDefinitions);
    }

    public static TableBuilder FromRows<T>(string tableName,
        IReadOnlyCollection<T> rows,
        IReadOnlyCollection<KustoColumnDefinition<T>> columnDefinitions)
    {
        return new TableBuilder(tableName,
            columnDefinitions.Select(Create).ToArray(),
            columnDefinitions.Select(c => c.Name).ToArray(),
            rows.Count);

        BaseColumn Create(KustoColumnDefinition<T> c)
        {
            var builder = ColumnHelpers.CreateBuilder(c.Type);
            foreach (var r in rows)
            {
                builder.Add(c.Value(r));
            }

            return builder.ToColumn();
        }
    }

    public static TableBuilder FromOrderedDictionarySet(string tableName,
        IReadOnlyCollection<OrderedDictionary> dictionaries)
    {
        //currently we rely on the dictionary having valid types and avoiding null values in unfortunate places
        var firstDictionary = dictionaries.First();
        var headers = firstDictionary.Cast<DictionaryEntry>()
            .Select(de => de.Key.ToString())
            .Select(h => new { ColumnName = h, Type = firstDictionary[h].GetType() })
            .ToArray();
        var columnDefinitions = headers
            .Select(h => h switch
            {
                _ when h.Type == typeof(int) =>
                    new KustoColumnDefinition<int>(h.ColumnName, ScalarTypes.Int,
                        x => dictionaries.ElementAt(x)[h.ColumnName]),
                _ when h.Type == typeof(DateTime) =>
                    new KustoColumnDefinition<int>(h.ColumnName, ScalarTypes.DateTime,
                        x => dictionaries.ElementAt(x)[h.ColumnName]),
                _ when h.Type == typeof(double) =>
                    new KustoColumnDefinition<int>(h.ColumnName, ScalarTypes.Real,
                        x => dictionaries.ElementAt(x)[h.ColumnName]),
                _ => new KustoColumnDefinition<int>(h.ColumnName, ScalarTypes.String,
                    x => dictionaries.ElementAt(x)[h.ColumnName].ToString()),
            })
            .ToList();

        return FromRows(tableName,
            Enumerable.Range(0, dictionaries.Count).ToArray(),
            columnDefinitions);
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
        var syms = _columnNames.Zip(_columns).Select(cs =>
                new ColumnSymbol(cs.First, cs.Second.Type))
            .ToArray();
        var ts = new TableSymbol(Name, syms);
        return new InMemoryTableSource(ts, _columns.ToArray());
    }
}