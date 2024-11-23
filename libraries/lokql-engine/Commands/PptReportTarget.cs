using System.Diagnostics.CodeAnalysis;
using DocumentFormat.OpenXml.Drawing;
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

    private T[] FindMatches<T>(string name) where T:IShape =>
        name.IsBlank()
            ? []
            : _pres.Slides.SelectMany(s => s.Shapes.OfType<T>())
                .Where(p => p.Name == name)
                .ToArray();

    public void UpdateOrAddImage(string name, byte[] data)
    {
        var matchingPictures = FindMatches<IPicture>(name);
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

    public void UpdateOrAddText(string name, string text)
    {
        var matches = FindMatches<IShape>(name);
        if (matches.Any())
        {
            foreach (var p in matches) p.Text = text;
        }
        else
        {
            
        }
    }

    public void UpdateOrAddTable(string name,InteractiveTableExplorer explorer)
    {
        var res = explorer._prevResult;
        var matchingTables = FindMatches<ITable>(name);
        
        if (matchingTables.Any())
        {
            var requiredRows = res.RowCount+1;//add 1 for header
            var requiredColumns =res.ColumnCount;
            foreach (var existingTable in matchingTables)
            {
                while (existingTable.Rows.Count > requiredRows) existingTable.Rows.RemoveAt(0);
                while (existingTable.Rows.Count < requiredRows) existingTable.Rows.Add();
                while (existingTable.Columns.Count > requiredColumns) existingTable.RemoveColumnAt(0);
                //while (existingTable.Columns.Count> requiredColumns) .../*what goes here? */ ;
                FillTable(existingTable);

            }
        }
        else
        {
            var slide = AddSlide();
            var shapeCollection = slide.Shapes;
           
            shapeCollection.AddTable(10, 10, res.ColumnCount, res.RowCount + 1);
            var newTable = (ITable)shapeCollection.Last();
            FillTable(newTable);
        }

        return;

        void FillTable(ITable addedTable)
        {
            var col = 0;
            var maxColumns = Math.Min(res.ColumnCount, addedTable.Columns.Count);
            foreach (var header in res.ColumnNames().Take(maxColumns))
            {
                var cell = addedTable[0, col];
                cell.TextBox.Text = header;
                col++;
            }

           
            for (var c = 0; c < maxColumns; c++)
            {
                for (var r = 0; r < res.RowCount; r++)
                {
                    var cell = addedTable[r + 1, c];
                    cell.TextBox.Text = res.Get(c, r)?.ToString() ?? "<null>";
                }
            }
        }
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
