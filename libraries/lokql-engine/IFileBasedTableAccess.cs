using KustoLoco.Core;
using KustoLoco.FileFormats;

namespace Lokql.Engine;

/// <summary>
/// provides a way to load and save tables from files
/// </summary>
/// <remarks>
/// The interface is very similar to ITableSerializer but changes the semantics slightly so
/// that tables are loaded into the supplied context.  It also requires the implementation to provide
/// a list of file extensions that will be used to determine whether the implementation should be used
/// based on the user's choice of path
/// </remarks>
public interface IFileBasedTableAccess
{
    public Task<bool> TryLoad(string path, KustoQueryContext context, string name);
    public IReadOnlyCollection<string> SupportedFileExtensions();
    public Task<bool> TrySave(string path, KustoQueryResult result);
    TableAdaptorDescription GetDescription();
}
