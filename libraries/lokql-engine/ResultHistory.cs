using KustoLoco.Core;

namespace Lokql.Engine;

public class ResultHistory
{
    private Dictionary<string,KustoQueryResult> _results= new ();
    public KustoQueryResult _mostRecent = KustoQueryResult.Empty;

    public void Push(KustoQueryResult result)
    {
        _mostRecent=result;
    }

    public void Save(string name)
    {
        _results[name] = _mostRecent;
    }

    public KustoQueryResult Fetch(string name) =>
        _results.TryGetValue(name, out var found)
            ? found
            : KustoQueryResult.Empty;

    public KustoQueryResult MostRecent => _mostRecent;

}
