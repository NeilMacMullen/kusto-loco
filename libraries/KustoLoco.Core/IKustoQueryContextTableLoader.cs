using System.Collections.Generic;
using System.Threading.Tasks;

namespace KustoLoco.Core;


/// <summary>
///     Interface to allow KustoQueryContext to lazily load tables as required
/// </summary>
/// <remarks>
///     It's important to understand that it is the _tableloader_ that is responsible
///     for correctly pipelining the addition of tables since the context is not
///     inherently thread-safe.
///
///     It is also up to the loader to determine policy for overwriting/renewing tables
/// </remarks>
public interface IKustoQueryContextTableLoader
{
    /// <summary>
    ///     Called with a list of table names referenced by a query
    /// </summary>
    /// <remarks>
    /// The list provided is a complete set of required tables with escaping/framing removed.
    /// It is up to the loader to determine if the tables are already present and if so, whether to reload them
    /// </remarks>
    public Task LoadTablesAsync(KustoQueryContext context, IReadOnlyCollection<string> tableNames);
}