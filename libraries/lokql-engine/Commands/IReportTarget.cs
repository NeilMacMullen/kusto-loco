using KustoLoco.Core;

namespace Lokql.Engine.Commands;

public interface IReportTarget
{
    void UpdateOrAddImage(string name,InteractiveTableExplorer explorer,KustoQueryResult result);
    void UpdateOrAddText(string name, string text);
    void SaveAs(string name);
    public void UpdateOrAddTable(string name,KustoQueryResult result);
}
