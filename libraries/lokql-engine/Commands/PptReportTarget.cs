using KustoLoco.Core;
using NotNullStrings;
using ShapeCrawler;

namespace Lokql.Engine.Commands;

public class PptReportTarget : IReportTarget
{
    private readonly Presentation _pres;

    private PptReportTarget(Presentation pres)
    {
        _pres = pres;
    }

    public async Task UpdateOrAddImage(string name, InteractiveTableExplorer explorer, KustoQueryResult result)
    {
        var surface = explorer.GetRenderingSurface();
        await UpdateOrAddImage(name, surface, result);
    }

    public void UpdateOrAddText(string name, string text)
    {
        var matches = FindMatches<IShape>(name);
        if (matches.Any())
            foreach (var p in matches)
                p.Text = text;
    }

    public void UpdateOrAddTable(string name, KustoQueryResult res)
    {
        var matchingTables = FindMatches<ITable>(name);

        if (matchingTables.Any())
        {
            var requiredRows = AllowableRowCount(res.RowCount + 1); //add 1 for header
            var requiredColumns = AllowableColumnCount(res.ColumnCount);
            foreach (var existingTable in matchingTables)
            {
                while (existingTable.Rows.Count > requiredRows) existingTable.Rows.RemoveAt(0);
                while (existingTable.Rows.Count < requiredRows) existingTable.Rows.Add();
                while (existingTable.Columns.Count > requiredColumns) existingTable.Columns.RemoveAt(0);
                while (existingTable.Columns.Count < requiredColumns) existingTable.Columns.Add();
                FillTable(existingTable);
            }
        }
        else
        {
            var slide = AddSlide();
            var shapeCollection = slide.Shapes;

            shapeCollection.AddTable(10, 10, res.ColumnCount, res.RowCount + 1);
            var newTable = (ITable)shapeCollection.Last();
            newTable.Name = name;
            FillTable(newTable);
        }

        return;

        void FillTable(ITable addedTable)
        {
            var col = 0;
            var maxColumns = AllowableColumnCount(Math.Min(res.ColumnCount, addedTable.Columns.Count));
            foreach (var header in res.ColumnNames().Take(maxColumns))
            {
                var cell = addedTable[0, col];
                cell.TextBox.Text = header;
                col++;
            }


            for (var c = 0; c < maxColumns; c++)
            for (var r = 0; r < AllowableRowCount(res.RowCount); r++)
            {
                var cell = addedTable[r + 1, c];
                cell.TextBox.Text = res.Get(c, r)?.ToString() ?? "<null>";
            }
        }
    }


    public void SaveAs(string name)
    {
        _pres.SaveAs(name);
    }

    private async Task UpdateOrAddImage(string name, IResultRenderingSurface surface, KustoQueryResult result)
    {
        var matchingPictures = FindMatches<IPicture>(name);
        if (matchingPictures.Any())
        {
            foreach (var p in matchingPictures)
            {
                var data = await surface.RenderToImage(result, (double)p.Width, (double)p.Height);
                p.Image!.Update(new MemoryStream(data));
            }
        }
        else
        {
            var slide = AddSlide();
            var data = await surface.RenderToImage(result, 800, 600);
            slide.Shapes.AddPicture(new MemoryStream(data));
            slide.Shapes.Last().Name = name;
        }
    }

    private int AllowableColumnCount(int wanted)
    {
        return Math.Min(wanted, 100);
    }

    private int AllowableRowCount(int wanted)
    {
        return Math.Min(wanted, 100);
    }

    private ISlide AddSlide()
    {
        _pres.Slides.AddEmptySlide(SlideLayoutType.Blank);
        return _pres.Slides.Last();
    }

    private T[] FindMatches<T>(string name) where T : IShape
    {
        return name.IsBlank()
            ? []
            : _pres.Slides.SelectMany(s => s.Shapes.OfType<T>())
                .Where(p => p.Name == name)
                .ToArray();
    }

    public static PptReportTarget Create(string filename)
    {
        var pres = filename.IsBlank()
            ? new Presentation()
            : new Presentation(filename);
        return new PptReportTarget(pres);
    }
}
