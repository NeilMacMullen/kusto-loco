namespace KustoSupport;

/// <summary>
///     Provides escaping for  names of tables and columns.
/// </summary>
/// <remarks>
///     This is actually the same scheme that JSON uses.  E.g.
///     ['my table'] actually represents a table called "my table"
///     This is particularly useful in the client layer where we
///     want to be able to perform lazy-loads based on the idea that the
///     table name contains the path to the actual file such as
///     ['c:/kusto/mydata.csv']
/// </remarks>
public static class KustoNameEscaping
{
    /// <summary>
    ///     Remove top layer of escaping from name
    /// </summary>
    public static string RemoveFraming(string tableName)
    {
        if ((tableName.StartsWith("['") && tableName.EndsWith("']")) ||
            (tableName.StartsWith("[\"") && tableName.EndsWith("\"]"))
           )
            return tableName.Substring(2, tableName.Length - 4);
        return tableName;
    }

    /// <summary>
    ///     Ensure we provide escape framing
    /// </summary>
    /// <remarks>
    ///     This isn't quite the same thing as true escaping - we are just assuming that
    ///     we want to the "escaped" form and if the current form is already escaped we
    ///     remove the framing first
    /// </remarks>
    public static string EnsureFraming(string tableName) => $"['{RemoveFraming(tableName)}']";
}