using KustoLoco.Core;
using KustoLoco.FileFormats;
using NotNullStrings;

namespace Lokql.Engine;

public abstract class TableAdaptorBase(ITableSerializer serializer,string name,string description,string extensions)
    : IFileBasedTableAccess
{
    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name)
    {
        var result = await serializer.LoadTable(path, name);
        context.AddTable(result.Table);
        return true;
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
