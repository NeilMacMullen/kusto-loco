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

    private ISlide AddSlide()
    {
        _pres.Slides.AddEmptySlide(SlideLayoutType.Blank);
        return _pres.Slides.Last();
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
            var slide = AddSlide();

            slide.Shapes.AddPicture(new MemoryStream(data));
        }
    }

    public void UpdateOrAddTable(string name,InteractiveTableExplorer explorer)
    {
        var slide = AddSlide();
        var shapeCollection=slide.Shapes;
        var res = explorer._prevResult;
        shapeCollection.AddTable(10, 10, res.ColumnCount, res.RowCount+1);
        var addedTable = (ITable)shapeCollection.Last();
        var col = 0;
        foreach (var header in res.ColumnNames())
        {
            var cell = addedTable[0, col];
            cell.TextBox.Text =header;
            col++;
        }

        for (var c = 0; c < res.ColumnCount; c++)
        {
            for (var r = 0; r < res.RowCount; r++)
            {
                var cell = addedTable[r + 1, c];
                cell.TextBox.Text = res.Get(c,r)?.ToString() ?? "<null>";
            }
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
