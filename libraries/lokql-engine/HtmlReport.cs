using KustoLoco.Rendering;
using Lokql.Engine.Commands;

namespace Lokql.Engine;

public class HtmlReport:IReportTarget
{
    public VegaComposer Composer { get; set; }

    public HtmlReport(string title)
    {
        Composer = new VegaComposer(title,"dark");
    }

    public string Render()
    {
        return Composer.Render();
    }

    public void UpdateOrAddImage(string name, InteractiveTableExplorer exp)
    {
        var result = exp._prevResult;
        var renderer = new KustoResultRenderer(exp.Settings);
        renderer.RenderToComposer(result, Composer);
    }

    public void UpdateOrAddImage(string name, byte[] data)
    {
        throw new NotImplementedException();
    }

    public void UpdateOrAddText(string name, string text)
    {
        throw new NotImplementedException();
    }

    public void SaveAs(string name)
    {
        var text = Render();
        File.WriteAllText(name, text);
        
    }

    public void UpdateOrAddTable(string name, InteractiveTableExplorer explorer)
    {
        throw new NotImplementedException();
    }
}
