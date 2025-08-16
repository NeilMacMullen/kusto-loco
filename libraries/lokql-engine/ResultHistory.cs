using System.Collections.Immutable;
using KustoLoco.Core;
using KustoLoco.PluginSupport;
using NotNullStrings;

namespace Lokql.Engine;

public class ResultHistory:IResultHistory
{
    private ImmutableList<StoredResult> _results = [];


    public KustoQueryResult MostRecent { get; private set; } = KustoQueryResult.Empty;


    public void Push(KustoQueryResult result)
    {
        MostRecent = result;
    }

    public void Store(string name)
    {
        _results = _results
            .Where(r=>! r.Name.Equals(name,StringComparison.InvariantCultureIgnoreCase))
            .Append(new StoredResult(name, DateTime.Now, MostRecent))
            .ToImmutableList();
        
    }
    private readonly StoredResult _nullStoredResult = new(string.Empty,
    DateTime.MinValue, KustoQueryResult.Empty);

    /// <summary>
    /// Get a stored result by name
    /// </summary>
    /// <remarks>
    /// If the name is empty or '_', return the most recent result
    /// </remarks>
    public KustoQueryResult Fetch(string name)
    {
        return name.IsBlank() || name == "_"
            ? MostRecent
            : _results.Where(r => r.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                .Append(_nullStoredResult)
                .First(_ => true).Result;

    }

    public ImmutableList<StoredResult> List()
    {
        return _results;
    }

    public readonly record struct StoredResult(string Name, DateTime Timestamp, KustoQueryResult Result);
}
