namespace Lokql.Engine.Commands;

public interface IReportTarget
{
    void UpdateOrAddImage(string name,InteractiveTableExplorer explorer);
    void UpdateOrAddImage(string name, byte[] data);
    //void UpdateOrAddTable(string name, KustoQueryResult);
    void UpdateOrAddText(string name, string text);
    void SaveAs(string name);
    public void UpdateOrAddTable(string name, InteractiveTableExplorer explorer);
}
