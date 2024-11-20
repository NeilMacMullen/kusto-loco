namespace Lokql.Engine.Commands;

public interface IReportTarget
{
    void UpdateOrAddImage(string name, byte[] data);
    //void UpdateOrAddTable(string name, KustoQueryResult);
    void UpdateOrAddText(string name, string text);
    void SaveAs(string name);
}
