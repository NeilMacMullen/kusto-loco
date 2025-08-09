using Kusto.Language.Symbols;
using KustoLoco.Core.Console;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Util;
using NLog;
using NotNullStrings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

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

    private ImmutableArray<string> _columnNames;

    private ImmutableArray<BaseColumn> _columns;

    private TableBuilder(string name, IEnumerable<BaseColumn> columns,
        IEnumerable<string> columnNames, int length)
    {
        _columns = [..columns];
        _columnNames = [..columnNames];
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
        for (var i = 1; i < 1000; i++)
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
    /// Creates a column from collection of items where we know the type
    /// </summary>
    public TableBuilder WithColumn(string name, Type type, IReadOnlyCollection<object?> items)
    {
        //TODO - since we know the length of the column we could use a more efficient allocation scheme in the builder
        //TODO - in fact this is only currently called from places we have already forced a ToArray so
        //we're doing a double copy here
        var builder = ColumnHelpers.CreateBuilder(type, name);
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
    public TableBuilder WithIndexColumn(string name, int value)
    {
        var indexColumn = new GenericSingleValueColumnOfint(value, Length);
        return WithColumn(name, indexColumn);
    }

    public static TableBuilder CreateFromVolatileData<T>(string tableName, IReadOnlyCollection<T> records)
        => CreateFromVolatileData(tableName, records, []);

    /// <summary>
    /// Creates a TableBuilder from a collection of records by _copying_ the data into new columns
    /// </summary>
    /// <remarks>
    /// This method is less efficient than <see cref="CreateFromImmutableData{T}"/> since it requires copying the data into new arrays.
    /// It's a little more flexible though and the overhead is generally not a problem for small collections (a few 1000s) of records.
    /// </remarks>
    public static TableBuilder CreateFromVolatileData<T>(string tableName, IReadOnlyCollection<T> records,IReadOnlyCollection<IKustoTypeConverter> overrides)
    {
        var builder = CreateEmpty(tableName, records.Count);
        foreach (var p in typeof(T).GetProperties())
        {
            var data = Array.Empty<object?>();
            var ov = overrides.FirstOrDefault(o => o.SourceType == p.PropertyType);
            if (ov != null)
            {
                data = records.Select(r => ov.Convert(p.GetValue(r))).ToArray();

                builder.WithColumn(p.Name, ov.TargetType, data);
                continue;
            }

            if (TypeMapping.IsImportableType(p.PropertyType))
            {
                data = records.Select(r => p.GetValue(r)).ToArray();
                builder.WithColumn(p.Name, p.PropertyType, data);
                continue;
            }
            data = records.Select(r => p.GetValue(r)?.ToString()??string.Empty).Cast<object?>().ToArray();
            builder.WithColumn(p.Name, typeof(string), data);

        }

        return builder;
    }


    public static TableBuilder CreateFromImmutableData<T>(string tableName, ImmutableArray<T> records)
        => CreateFromImmutableData(tableName, records, []);
    /// <summary>
/// Creates columns that wrap row properties in a lambda value getter
/// </summary>
/// <remarks>
/// This method allows us to create very efficient columns since the only overhead over the original data is the method call
/// </remarks>
    public static TableBuilder CreateFromImmutableData<T>(string tableName, ImmutableArray<T> records,IKustoTypeConverter[] overrides)
    {
        var builder = CreateEmpty(tableName, records.Length);

        var columnActions = new Dictionary<Type, Action<string,Func<T,object?>>>
        {
            [typeof(short)] = (name, fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfint<T>(records, o => (int?)fn(o))),
            [typeof(ushort)] = (name, fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfint<T>(records, o => (int?)fn(o))),
            [typeof(int)] = (name,fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfint<T>(records, o => (int?)fn(o))),
            [typeof(uint)] = (name, fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfint<T>(records, o => (int?)fn(o))),
            [typeof(long)] = (name,fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOflong<T>(records, o => (long?)fn(o))),
            [typeof(ulong)] = (name, fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOflong<T>(records, o => (long?)fn(o))),

            [typeof(float)] = (name,fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfdouble<T>(records, o => (double?)fn(o))),
            [typeof(double)] = (name,fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfdouble<T>(records, o => (double?)fn(o))),
            [typeof(decimal)] = (name,fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfdecimal<T>(records, o => (decimal?)fn(o))),
            [typeof(string)] = (name,fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfstring<T>(records, o => (string?)fn(o) ??string.Empty )),
            [typeof(DateTime)] = (name,fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfDateTime<T>(records, o => (DateTime?)fn(o))),
            [typeof(TimeSpan)] = (name,fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfTimeSpan<T>(records, o => (TimeSpan?)fn(o))),
            [typeof(bool)] = (name,fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfbool<T>(records, o => (bool?)fn(o))),
            [typeof(Guid)] = (name,fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfGuid<T>(records, o => (Guid?)fn(o))),
            [typeof(JsonNode)] = (name, fn) => builder.WithColumn(name, new GenericLambdaWrappedColumnOfJsonNode<T>(records, o => (Guid?)fn(o))),
        };

        foreach (var p in typeof(T).GetProperties())
        {
            var ov = overrides.FirstOrDefault(o =>
                    o.SourceType == p.PropertyType);
            if (ov != null)
            {
                if (columnActions.TryGetValue(ov.TargetType, out var make))
                {
                    make(p.Name, o => ov.Convert( p.GetValue(o)));
                }
                continue;
            }
            //fallthrough to standard type mapping
            var propertyType = p.PropertyType;
            if (columnActions.TryGetValue(propertyType, out var action))
            {
                action(p.Name,o=>p.GetValue(o));
            }
            else
            {
                //if nothing else, convert to string
                builder.WithColumn(p.Name,
                    new GenericLambdaWrappedColumnOfstring<T>(records, string? (o) => p.GetValue(o)?.ToString() ?? string.Empty));
            }
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
    /// Creates a TableBuilder instance from a DataTable, populating it with the data from the table's rows and columns.
    /// </summary>
    /// <param name="dataTable">The source of data that will be used to populate the new table structure.</param>
    /// <param name="tableName">Specifies the name of the table, defaulting to the source's name if not provided.</param>
    /// <returns>Returns a TableBuilder populated with the data from the provided DataTable.</returns>
    public static TableBuilder FromDataTable(
        DataTable dataTable, string tableName)
    {
        tableName = tableName.OrWhenBlank(dataTable.TableName);
        //currently we rely on the dictionary having valid types and avoiding null values in unfortunate places
        var columnBuilders = dataTable.Columns.Cast<DataColumn>()
            .Select(h => ColumnHelpers.CreateBuilder(h.DataType,h.ColumnName))
            .ToArray();

        var tableBuilder = CreateEmpty(tableName,dataTable.Rows.Count);

        foreach (DataRow row  in dataTable.Rows)
        {
            for (var i = 0; i < columnBuilders.Length; i++)
            {
                var cb = columnBuilders[i];
                cb.Add(row[i]);
            }
        }
        foreach(var b in columnBuilders)
            tableBuilder.WithColumn(b.Name,b.ToColumn());

        return tableBuilder;
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
