using KustoLoco.Core;
using KustoLoco.FileFormats;
using NotNullStrings;

namespace Lokql.Engine;

public abstract class TableAdaptorBase(ITableSerializer serializer,string name,string description,string extensions)
    : IFileBasedTableAccess
{
    public async Task<TableLoadResult> TryLoad(string path,string tableName)
    {
        var result = await serializer.LoadTable(path, tableName);
        return result;
    }

    public async Task<bool> TrySave(string path, KustoQueryResult result)
    {
        return (await serializer.SaveTable(path, result)).Error.IsBlank();
    }

 
    public IReadOnlyCollection<string> SupportedFileExtensions()
    {
        return extensions.Tokenize().Select(x=>$".{x}").ToArray();
    }

    public TableAdaptorDescription GetDescription()
    {
        return new TableAdaptorDescription(name, description, SupportedFileExtensions());
    }
}
