using KustoLoco.Core;
using NotNullStrings;

namespace Lokql.Engine;

public class ResultHistory
{
    public KustoQueryResult _mostRecent = KustoQueryResult.Empty;
    private readonly Dictionary<string, KustoQueryResult> _results = new();


    public KustoQueryResult MostRecent => _mostRecent;

    public void Push(KustoQueryResult result)
    {
        _mostRecent = result;
    }

    public void Save(string name)
    {
        _results[name] = _mostRecent;
    }
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
            : _results.TryGetValue(name, out var found)
                ? found
                : KustoQueryResult.Empty;
    }
}
