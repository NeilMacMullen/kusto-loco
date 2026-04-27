using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Util;

using NotNullStrings;

namespace KustoLoco.Core;

public class KustoQueryResult
{
    

    public static readonly KustoQueryResult Empty = new(string.Empty,
        InMemoryTableSource.Empty, VisualizationState.Empty, TimeSpan.Zero, string.Empty);

    /// <summary>
    ///     Provides the results of a Kusto query as a collection of dictionaries
    /// </summary>
    /// <remarks>
    ///     The client really needs to have some idea of the expected schema to use this layer of API
    ///     In the future we may provide a more "column-oriented" result where the type of the columns
    ///     is explicitly provided but for the moment this allows easy json serialisation
    /// </remarks>
    public KustoQueryResult(string query,
        IMaterializedTableSource results,
        VisualizationState vis,
        TimeSpan queryDuration,
        string error)
    {
        Query = query;
        Table = results;
        Visualization = vis;
        RowCount = results.RowCount;

        QueryDuration = queryDuration;
        Error = error;
    }

    /// <summary>
    ///     Original query that generated the result
    /// </summary>
    public string Query { get; init; }


    public IMaterializedTableSource Table { get; }

    /// <summary>
    ///     Duration of query execution
    /// </summary>
    public TimeSpan QueryDuration { get; init; }

    /// <summary>
    ///     Error message if the query failed, otherwise string.Empty
    /// </summary>
    public string Error { get; init; }

    /// <summary>
    ///     Visualization state of the result (i.e. whether it's a table, chart, etc)
    /// </summary>
    public VisualizationState Visualization { get; init; }

    /// <summary>
    ///     Number of rows in the result
    /// </summary>
    public int RowCount { get; init; }

    /// <summary>
    ///     Number of columns in the result
    /// </summary>
    public int ColumnCount => ColumnDefinitions().Length;

    /// <summary>
    /// True if the result should be visualised as a chart 
    /// </summary>
    public bool IsChart =>
        Visualization.ChartType.IsNotBlank() && Visualization.ChartType.ToLowerInvariant() != "table";

    public static KustoQueryResult FromError(string query, string error)
    {
        return new KustoQueryResult(query, InMemoryTableSource.Empty, VisualizationState.Empty, TimeSpan.Zero, error);
    }

    /// <summary>
    ///     Gets an array of nullable objects representing cells in the row at the given index
    /// </summary>
    /// <remarks>
    ///     Kusto uses null to represent missing values so we need to use nullable types here
    /// </remarks>
    public object?[] GetRow(int row)
    {
        return Enumerable.Range(0, ColumnCount)
            .Select(c => Get(c, row))
            .ToArray();
    }

    /// <summary>
    ///     Enumerate over all rows in the result
    /// </summary>
    public IEnumerable<object?[]> EnumerateRows(int maxCount)
    =>  Enumerable.Range(0, Math.Min(RowCount,maxCount))
            .Select(GetRow);

    public IEnumerable<object?[]> EnumerateRows()
        => EnumerateRows(RowCount);

    /// <summary>
    ///     Returns the column definitions for the result
    /// </summary>
    /// <remarks>
    ///     A ColumnResult is a simple struct that contains the name index and underlying C# type of a column
    /// </remarks>
    public ColumnResult[] ColumnDefinitions()
    {
        return Table.Type
            .Columns
            .Select((c, i) => new ColumnResult(
                c.Name,
                i,
                TypeMapping.UnderlyingTypeForSymbol(c.Type))
            )
            .ToArray();
    }

    /// <summary>
    ///     Returns the names of the columns in the result
    /// </summary>
    public string[] ColumnNames()
    {
        return ColumnDefinitions().Select(c => c.Name).ToArray();
    }

    /// <summary>
    ///     Fetch a particular cell in the result
    /// </summary>
    public object? Get(int col, int row)
    {
        var chunk = Table.GetData().First();
        return chunk.Columns[col].GetRawDataValue(row);
    }

    public IReadOnlyCollection<OrderedDictionary> AsOrderedDictionarySet(int max = int.MaxValue)
    {
        var items = new List<OrderedDictionary>();

        var columns = ColumnDefinitions();
        var chunk = Table.GetData().First();
        var rowsToTake = Math.Min(max, RowCount);
        for (var row = 0; row < rowsToTake; row++)
        {
            var d = RowToDictionary(row);
            items.Add(d);
        }

        return items;
    }

    public OrderedDictionary RowToDictionary(int row)
    {
        var d = new OrderedDictionary();
        var columns = ColumnDefinitions();
        for (var col = 0; col < columns.Length; col++)
        {
            var dataValue =Get(col,row);
            var columnName = columns[col].Name;
            d[columnName] = dataValue;
        }
        return d;

    }
    /// <summary>
    ///     For a particular column enumerate over all cells
    /// </summary>
    public IEnumerable<object?> EnumerateColumnData(ColumnResult col)
    {
        return Table.GetColumnData(col.Index);
    }

    /// <summary>
    ///     Cache for property mappings to avoid repeated reflection
    /// </summary>
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    /// <summary>
    ///     Cache for constructor parameter mappings for record types
    /// </summary>
    private static readonly ConcurrentDictionary<Type, ConstructorInfo?> ConstructorCache = new();

    /// <summary>
    ///     Efficiently maps query results to records using reflection
    /// </summary>
    /// <remarks>
    ///     This method directly maps column data to object properties without
    ///     going through JSON serialization, providing better performance.
    ///     Property matching is case-insensitive and also respects JsonPropertyName attributes.
    ///     Supports both mutable classes and immutable record structs.
    /// </remarks>
    public IReadOnlyCollection<T> ToRecords<T>(int max = int.MaxValue)
    {
        var results = new List<T>();
        var columns = ColumnDefinitions();
        var rowsToTake = Math.Min(max, RowCount);

        if (rowsToTake == 0)
            return results;

        var type = typeof(T);

        // Check if this is a record type with a primary constructor
        var primaryConstructor = ConstructorCache.GetOrAdd(type, t => GetPrimaryConstructor(t));

        if (primaryConstructor != null)
        {
            // Handle record types with primary constructors
            var parameters = primaryConstructor.GetParameters();
            var properties = PropertyCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .ToArray());

            // Build a mapping from column names to properties
            var propertyLookup = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in properties)
            {
                propertyLookup[prop.Name] = prop;

                var jsonAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (jsonAttr != null)
                {
                    propertyLookup[jsonAttr.Name] = prop;
                }
            }

            // Create column-to-parameter mapping
            var parameterMappings = parameters
                .Select(param =>
                {
                    // Find the column index for this parameter by matching with properties
                    var columnIndex = -1;

                    // Try to find matching property for this parameter (case-insensitive)
                    var matchingProperty = properties.FirstOrDefault(p =>
                        string.Equals(p.Name, param.Name, StringComparison.OrdinalIgnoreCase));

                    if (matchingProperty != null)
                    {
                        // Find column by property name
                        for (var i = 0; i < columns.Length; i++)
                        {
                            if (string.Equals(columns[i].Name, matchingProperty.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                columnIndex = i;
                                break;
                            }
                        }

                        // If not found, check JsonPropertyName attribute
                        if (columnIndex == -1)
                        {
                            var jsonAttr = matchingProperty.GetCustomAttribute<JsonPropertyNameAttribute>();
                            if (jsonAttr != null)
                            {
                                for (var i = 0; i < columns.Length; i++)
                                {
                                    if (string.Equals(columns[i].Name, jsonAttr.Name, StringComparison.OrdinalIgnoreCase))
                                    {
                                        columnIndex = i;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    return (Parameter: param, ColumnIndex: columnIndex);
                })
                .ToArray();

            for (var row = 0; row < rowsToTake; row++)
            {
                var args = new object?[parameters.Length];
                for (var i = 0; i < parameterMappings.Length; i++)
                {
                    var (param, colIndex) = parameterMappings[i];
                    if (colIndex >= 0)
                    {
                        var value = Get(colIndex, row);
                        args[i] = value != null ? ConvertValue(value, param.ParameterType) : null;
                    }
                    else
                    {
                        // Use default value if column not found
                        args[i] = param.HasDefaultValue ? param.DefaultValue : GetDefaultValue(param.ParameterType);
                    }
                }
                var record = (T)primaryConstructor.Invoke(args);
                results.Add(record);
            }
        }
        else
        {
            // Handle traditional mutable classes
            // Get writable properties with caching
            var properties = PropertyCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite)
                    .ToArray());

            // Build a mapping from column names to properties
            // Supports both property names and JsonPropertyName attributes
            var propertyLookup = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in properties)
            {
                // Add by property name
                propertyLookup[prop.Name] = prop;

                // Also check for JsonPropertyName attribute
                var jsonAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (jsonAttr != null)
                {
                    propertyLookup[jsonAttr.Name] = prop;
                }
            }

            // Create column-to-property mapping
            var columnMappings = columns
                .Select((col, index) => (
                    Index: index,
                    Column: col,
                    Property: propertyLookup.GetValueOrDefault(col.Name)
                ))
                .Where(x => x.Property != null)
                .ToArray();

            for (var row = 0; row < rowsToTake; row++)
            {
                var record = Activator.CreateInstance<T>();
                foreach (var (colIndex, column, property) in columnMappings)
                {
                    var value = Get(colIndex, row);
                    if (value != null)
                    {
                        var convertedValue = ConvertValue(value, property!.PropertyType);
                        property!.SetValue(record, convertedValue);
                    }
                }
                results.Add(record);
            }
        }

        return results;
    }

    /// <summary>
    ///     Gets the primary constructor for a record type, or null if not a record or no suitable constructor
    /// </summary>
    private static ConstructorInfo? GetPrimaryConstructor(Type type)
    {
        // Get all public constructors
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        if (constructors.Length == 0)
            return null;

        // Get public properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Look for a constructor where all parameters have matching properties
        // This works for both record classes and record structs
        var primaryConstructor = constructors
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault(c =>
            {
                var parameters = c.GetParameters();
                if (parameters.Length == 0)
                    return false;

                // Check if all parameters have matching properties (case-insensitive name match)
                // and all parameters map to read-only or init-only properties (typical for records)
                var allMatch = parameters.All(p =>
                    properties.Any(prop =>
                        string.Equals(prop.Name, p.Name, StringComparison.OrdinalIgnoreCase)));

                if (!allMatch)
                    return false;

                // Additional check: if all properties are read-only (no public setter),
                // or if the type has no parameterless constructor, this is likely a record type
                var hasParameterlessConstructor = constructors.Any(c => c.GetParameters().Length == 0);
                var allPropertiesReadOnly = properties.All(p => !p.CanWrite || 
                    p.SetMethod?.ReturnParameter?.GetRequiredCustomModifiers()
                        .Any(t => t.Name == "IsExternalInit") == true);

                return !hasParameterlessConstructor || allPropertiesReadOnly;
            });

        return primaryConstructor;
    }

    /// <summary>
    ///     Gets the default value for a type
    /// </summary>
    private static object? GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    /// <summary>
    ///     Converts a value to the target type, handling nullable types and common conversions
    /// </summary>
    private static object? ConvertValue(object value, Type targetType)
    {
        var underlyingType = TypeMapping.UnderlyingType(targetType);

        // Direct assignment if types match
        if (value.GetType() == underlyingType || underlyingType.IsAssignableFrom(value.GetType()))
            return value;

        // Handle enum conversion from string or numeric types
        if (underlyingType.IsEnum)
        {
            if (value is string stringValue)
                return Enum.Parse(underlyingType, stringValue, ignoreCase: true);
            return Enum.ToObject(underlyingType, value);
        }

        // Use Convert.ChangeType for compatible type conversions
        return Convert.ChangeType(value, underlyingType);
    }

    [Obsolete("Use ToRecords<T>() instead. This method will be removed in a future version.")]
    public static IReadOnlyCollection<T> ToRecordsViaJson<T>(IEnumerable<OrderedDictionary> dictionaries)
    {
        var json = ToJsonString(dictionaries);
        try
        {
            return JsonSerializer.Deserialize<T[]>(json)!;
        }
        catch
        {
            return [];
        }
    }

    public string ToJsonString()
    {
        return ToJsonString(AsOrderedDictionarySet());
    }

    public static string ToJsonString(object o)
    {
        return JsonSerializer.Serialize(o);
    }

    public IReadOnlyCollection<OrderedDictionary> ResultOrErrorAsSet()
    {
        return Error.IsNotBlank()
            ? [new OrderedDictionary { ["QUERY ERROR"] = Error }]
            : AsOrderedDictionarySet();
    }


    /// <summary>
    ///     Allows a KustoQueryResult to dropped into a DTO or other serializable object
    /// </summary>
    /// <remarks>
    ///     We currently use an OrderedDictionary to represent the rows of the result set.
    ///     However, in future we may return a JsonObject
    /// </remarks>
    public object ToSerializableObject()
    {
        return AsOrderedDictionarySet();
    }

    /// <summary>
    ///     Returns the result of a query as a DataTable, suitable for use in a DataGridView or similar
    /// </summary>
    public DataTable ToDataTable(int maxRows = int.MaxValue)
    {
        var dt = new DataTable();

        foreach (var col in ColumnDefinitions())
            dt.Columns.Add(col.Name, col.UnderlyingType);

        foreach (var row in EnumerateRows().Take(maxRows))
            dt.Rows.Add(row);

        return dt;
    }

    /// <summary>
    ///     Returns the result of a query as a DataTable, suitable for use in a DataGridView or similar
    /// </summary>
    /// <remarks>
    ///     If the result has an error, returns a single cell DataTable that contains the error text
    /// </remarks>
    public DataTable ToDataTableOrError(int maxRows = int.MaxValue)
    {
        if (Error.IsNotBlank())
        {
            var dt = new DataTable();
            dt.Columns.Add("ERROR");
            dt.Rows.Add(Error);
            return dt;
        }

        return ToDataTable(maxRows);
    }

    /// <summary>
    ///     Returns a page of the result
    /// </summary>
    public KustoQueryResult GetPage(int offset, int count)
    {
        if (RowCount == 0)
            return this;
        var pageOffset = Math.Min(RowCount, offset);
        var pageCount = Math.Min(RowCount - pageOffset, count);
        var pagedTable = PageOfKustoTable.Create(Table, pageOffset, pageCount);
        return new KustoQueryResult(Query, InMemoryTableSource.FromITableSource(pagedTable), Visualization,
            QueryDuration,
            Error);
    }

    /// <summary>
    /// Break a single result into multiple pages
    /// </summary>
    /// <remarks>
    /// When transferring large result sets over the network it can be useful to break them into pages
    /// So for example a client can issue a query that would return 100,000 rows and then paginate over them
    /// </remarks>
    public IReadOnlyCollection<KustoQueryResult> Paginate(int pageSize)
    {
        var pages = new List<KustoQueryResult>();

        for (var start = 0; start < RowCount; start += pageSize)
        {
            var page = GetPage(start, pageSize);
            pages.Add(page);
        }

        return pages;
    }
}
