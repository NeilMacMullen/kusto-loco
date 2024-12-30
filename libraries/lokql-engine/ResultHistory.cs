using System.Collections.Immutable;
using KustoLoco.Core;
using NotNullStrings;

namespace Lokql.Engine;

public class ResultHistory
{
    private ImmutableList<StoredResult> _results = [];


    public KustoQueryResult MostRecent { get; private set; } = KustoQueryResult.Empty;


    public void Push(KustoQueryResult result)
    {
        MostRecent = result;
    }

    public void Save(string name)
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
    /// If the name is empty, return the most recent result
    /// </remarks>
    public KustoQueryResult Fetch(string name)
    {
        return name.IsBlank()
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
