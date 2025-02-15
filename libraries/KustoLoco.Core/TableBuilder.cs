using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Util;
using Kusto.Language.Symbols;
using KustoLoco.Core.Console;
using KustoLoco.Core.DataSource.Columns;
using NLog;
using KustoLoco.Core.DataSource;
using NotNullStrings;

namespace KustoLoco.Core;



/// <summary>
///     Provides a simple way to create a Kusto ITableSource
/// </summary>
/// <remarks>
/// A TableBuilder is a mutable object that allows you to create a Kusto ITableSource by adding columns to it.
/// </remarks>
public class TableBuilder
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public readonly int Length;
    public readonly string Name;

    private ImmutableArray<string> _columnNames ;

    private ImmutableArray<BaseColumn> _columns ;

    private TableBuilder(string name, IEnumerable<BaseColumn> columns,
        IEnumerable<string> columnNames, int length)
    {
        _columns = columns.ToImmutableArray();
        _columnNames = columnNames.ToImmutableArray();
        Length = length;
        Name = name;
    }

    /// <summary>
    /// Creates an empty TableBuilder with the given name and length
    /// </summary>
    /// <remarks>
    /// All columns within the table are required to have the same length so this needs to be declared
    /// here
    /// </remarks>
    public static TableBuilder CreateEmpty(string name, int length)
        => new(
            name,
            [],
            [],
            length);
    /// <summary>
    /// Try to ensure that added column names are unique and not blank
    /// </summary>
    public string GetUniqueColumnName(string name)
    {
        if (name.IsBlank())
            name = "_column";
       
        if (!_columnNames.Contains(name))
            return name;
        for(var i = 1; i < 1000;i++)
        {
            var newName = $"{name}_{i}";
            if (!_columnNames.Contains(newName))
                return newName;
        }
        //if we ever see this I shudder to think what the input must be
        throw new InvalidOperationException("can't find a unique column name amongst large number of duplicate names ");
    }
    /// <summary>
    /// Adds a column to the builder 
    /// </summary>
    /// <remarks>
    ///  Should only be used when the client code must deal with columns of data where there's
    /// very little advance type information
    /// </remarks>
    public TableBuilder WithColumn(string name, BaseColumn column)
    {
        name = GetUniqueColumnName(name);
        _columns = _columns.Add(column);
        _columnNames = _columnNames.Add(name);
        return this;
    }
    /// <summary>
    /// Adds a column of data to builder by copying it into a new array
    /// </summary>
    public TableBuilder WithColumn<T>(string name, IEnumerable<T> items)
    {
        var column = ColumnFactory.Create(items.ToArray());
        return WithColumn(name, column);
    }

    /// <summary>
    /// Creates a column from collection of items where we know the type
    /// </summary>
    public TableBuilder WithColumn(string name, Type type, IReadOnlyCollection<object?> items)
    {
        //TODO - since we know the length of the column we could use a more efficient allocation scheme in the builder
        //TODO - in fact this is only currently called from places we have already forced a ToArray so
        //we're doing a double copy here
        var builder = ColumnHelpers.CreateBuilder(type);
        foreach (var item in items)
        {
            builder.Add(item);
        }

        return WithColumn(name, builder.ToColumn());
    }

    /// <summary>
    /// Add an index column to the table 
    /// </summary>
    /// <remarks>
    /// An index column is a column that contains a single value repeated for the length of the table
    /// Typically used to provide efficient filtering and summarization operations
    /// </remarks>
    public TableBuilder WithIndexColumn<T>(string name, T? value)
    {
        var indexColumn = new SingleValueColumn<T>(value, Length);
        return WithColumn(name, indexColumn);
    }

  
    /// <summary>
    /// Creates a TableBuilder from a collection of records by _copying_ the data into new columns
    /// </summary>
    /// <remarks>
    /// This method is less efficient than <see cref="CreateFromImmutableData{T}"/> since it requires copying the data into new arrays.
    /// It's a little more flexible though and the overhead is generally not a problem for small collections (a few 1000s) of records.
    /// </remarks>
    public static TableBuilder CreateFromVolatileData<T>(string tableName, IReadOnlyCollection<T> records)
    {
        var builder = CreateEmpty(tableName, records.Count);
        foreach (var p in typeof(T).GetProperties())
        {
            var data = records.Select(r => p.GetValue(r)).ToArray();

            builder.WithColumn(p.Name, p.PropertyType, data);
        }

        return builder;
    }

    /// <summary>
    /// Creates columns that wrap row properties in a lambda value getter
    /// </summary>
    /// <remarks>
    /// This method allows us to create very efficient columns since the only overhead over the original data is the method call
    /// </remarks>
    public  static TableBuilder CreateFromImmutableData<T>(string tableName, ImmutableArray<T> records)
    {
        var builder = CreateEmpty(tableName, records.Length);
        foreach (var p in typeof(T).GetProperties())
        {
            var propertyType = p.PropertyType;
            if (propertyType == typeof(int))
                builder.WithColumn(p.Name, new LambdaWrappedColumn<T, int?>(records, o => (int?)p.GetValue(o)));
            if (propertyType == typeof(long))
                builder.WithColumn(p.Name, new LambdaWrappedColumn<T, long?>(records, o => (long?)p.GetValue(o)));
            if (propertyType == typeof(float))
                builder.WithColumn(p.Name, new LambdaWrappedColumn<T, double?>(records, o => (float?)p.GetValue(o)));
            if (propertyType == typeof(double))
                builder.WithColumn(p.Name, new LambdaWrappedColumn<T, double?>(records, o => (double?)p.GetValue(o)));
            if (propertyType == typeof(string))
                builder.WithColumn(p.Name, new LambdaWrappedColumn<T, string?>(records, o => (string?)p.GetValue(o)));
            if (propertyType == typeof(DateTime))
                builder.WithColumn(p.Name,
                    new LambdaWrappedColumn<T, DateTime?>(records, o => (DateTime?)p.GetValue(o)));
            if (propertyType == typeof(TimeSpan))
                builder.WithColumn(p.Name,
                    new LambdaWrappedColumn<T, TimeSpan?>(records, o => (TimeSpan?)p.GetValue(o)));
            if (propertyType == typeof(bool))
                builder.WithColumn(p.Name, new LambdaWrappedColumn<T, bool?>(records, o => (bool?)p.GetValue(o)));
            if (propertyType == typeof(Guid))
                builder.WithColumn(p.Name, new LambdaWrappedColumn<T, Guid?>(records, o => (Guid?)p.GetValue(o)));
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
            .Select(h => new
            {
                ColumnName = h!,
                Type = firstDictionary[h!]?.GetType() ?? typeof(string)
            })
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
    /// <summary>
    /// Produce a single-column, single-row table from a scalar result
    /// </summary>
    public static ITableSource FromScalarResult(string tableName,ScalarResult scalar)
    {
        var column = ColumnHelpers.CreateFromScalar(scalar.Value, scalar.Type, 1);
        return CreateEmpty(tableName, 1)
            .WithColumn("value", column)
            .ToTableSource();
    }

    public ITableSource ToTableSource()
    {
        var symbols = _columnNames.Zip(_columns)
            .Select(cs =>
                new ColumnSymbol(cs.First, cs.Second.Type))
            .ToArray();
        var ts = new TableSymbol(Name, symbols);
        return new InMemoryTableSource(ts, _columns.ToArray());
    }

    public static ITableSource FromTable(ITableSource table, string requestedTableName)
    {
        if (table is not InMemoryTableSource ims)
            throw new NotImplementedException("can currently only share generated tables");
        return ims.ShareAs(requestedTableName);
    }

    /// <summary>
    /// Creates a new table by inferring the types of the columns in the input table
    /// </summary>
    public static ITableSource AutoInferColumnTypes(ITableSource other,IKustoConsole console)
    {
        var chunks = other.GetData().ToArray();
        switch (chunks.Length)
        {
            case 0:
                return other;
            case > 1:
                throw new NotImplementedException("can currently only infer types for single chunk tables");
        }

        var columns = chunks[0].Columns;
        if (columns.Length == 0)
            return other;

        var columnNames = other.ColumnNames.ToArray();
        var inferredColumns = columns.Zip(columnNames, (col, name) =>
            {
                var newC = ColumnTypeInferrer.AutoInfer(col);
                console.ShowProgress($"Column {name} -> {newC.Type.Name}");
                return newC;
            })
            .ToArray();
        var builder = CreateEmpty(other.Name, columns[0].RowCount);
        for (var i = 0; i < columns.Length; i++)
        {
            builder.WithColumn(columnNames[i], inferredColumns[i]);
        }
        console.CompleteProgress("");
        return builder.ToTableSource();
    
    }
}
