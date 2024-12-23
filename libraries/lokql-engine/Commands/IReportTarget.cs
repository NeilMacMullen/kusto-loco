using KustoLoco.Core;

namespace Lokql.Engine.Commands;

public interface IReportTarget
{
    Task UpdateOrAddImage(string name,InteractiveTableExplorer explorer);
    void UpdateOrAddText(string name, string text);
    void SaveAs(string name);
    public void UpdateOrAddTable(string name,KustoQueryResult result);
}
