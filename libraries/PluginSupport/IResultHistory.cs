using KustoLoco.Core;

namespace KustoLoco.PluginSupport;

/// <summary>
/// Provides access to previous result history, allowing fetching and storing of query results by name.
/// </summary>
public interface IResultHistory
{
    /// <summary>
    /// Fetches a query result by its name.
    /// </summary>
    /// <param name="resultName">The name of the result to fetch.</param>
    /// <returns>The <see cref="KustoQueryResult"/> associated with the specified name.</returns>
    KustoQueryResult Fetch(string resultName);

    /// <summary>
    /// Stores a query result by name.
    /// </summary>
    /// <param name="name">The name under which to store the result.</param>
    void Store(string name);
}
