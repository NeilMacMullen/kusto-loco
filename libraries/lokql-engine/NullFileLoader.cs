using KustoLoco.Core;
using KustoLoco.FileFormats;

namespace Lokql.Engine;


public class NullFileLoader : IFileBasedTableAccess
{
    public Task<TableLoadResult> TryLoad(string path,  string name)
        => Task.FromResult(TableLoadResult.Failure("no tables available in Null File Loader"));

    public IReadOnlyCollection<string> SupportedFileExtensions() => [];

    public Task<bool> TrySave(string path, KustoQueryResult result) => Task.FromResult(false);

    public TableAdaptorDescription GetDescription()
        => new("None", "Not supported", SupportedFileExtensions());
}
