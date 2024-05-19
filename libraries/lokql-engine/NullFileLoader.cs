using KustoLoco.Core;
using KustoLoco.FileFormats;

namespace Lokql.Engine;


public class NullFileLoader : IFileBasedTableAccess
{
    public Task<bool> TryLoad(string path, KustoQueryContext context, string name)
        => Task.FromResult(false);

    public IReadOnlyCollection<string> SupportedFileExtensions() => [];

    public Task<bool> TrySave(string path, KustoQueryResult result) => Task.FromResult(false);

    public TableAdaptorDescription GetDescription()
        => new("None", "Not supported", SupportedFileExtensions());
}
