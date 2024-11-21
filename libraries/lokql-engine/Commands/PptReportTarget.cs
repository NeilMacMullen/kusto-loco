using NotNullStrings;
using ShapeCrawler;

namespace Lokql.Engine.Commands;

// see https://github.com/MicrosoftEdge/WebView2Feedback/issues/3453
public class PptReportTarget : IReportTarget
{
    private readonly Presentation _pres;

    private PptReportTarget(Presentation pres)
    {
        _pres = pres;
    }

    public void UpdateOrAddImage(string name, InteractiveTableExplorer explorer)
    {
        var bytes = explorer.GetImageBytes();
        UpdateOrAddImage(name, bytes);
    }

    public void UpdateOrAddImage(string name, byte[] data)
    {
        var matchingPictures = name.IsBlank()
            ? []
            : _pres.Slides.SelectMany(s => s.Shapes.OfType<IPicture>())
                .Where(p => p.Name == name)
                .ToArray();
        if (matchingPictures.Any())
        {
            foreach (var p in matchingPictures) p.Image!.Update(data);
        }
        else
        {
            _pres.Slides.AddEmptySlide(SlideLayoutType.Blank);
            var slide = _pres.Slides.Last();

            slide.Shapes.AddPicture(new MemoryStream(data));
        }
    }

    public void UpdateOrAddText(string name, string text)
    {
        throw new NotImplementedException();
    }

    public void SaveAs(string name)
    {
        _pres.SaveAs(name);
    }

    public static PptReportTarget Create(string filename)
    {
        var pres = filename.IsBlank()
            ? new Presentation()
            : new Presentation(filename);
        return new PptReportTarget(pres);
    }
}
