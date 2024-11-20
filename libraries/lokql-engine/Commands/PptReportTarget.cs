using ShapeCrawler;

namespace Lokql.Engine.Commands;
// see https://github.com/MicrosoftEdge/WebView2Feedback/issues/3453
public class PptReportTarget : IReportTarget
{
    private readonly Presentation _pres;

    public void UpdateOrAddImage(string name, byte[] data)
    {

        var matchingPictures = _pres.Slides.SelectMany(s => s.Shapes.OfType<IPicture>())
            .Where(p => p.Name == name)
            .ToArray();
        if (matchingPictures.Any())
        {
            foreach (var p in matchingPictures)
            {
                p.Image.Update(data);

            }
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
        var pres = new Presentation(filename);
        return new PptReportTarget(pres);
    }
    private PptReportTarget(Presentation pres)
    {
        this._pres = pres;
    }
}
