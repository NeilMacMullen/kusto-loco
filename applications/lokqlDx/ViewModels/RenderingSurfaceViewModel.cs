using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Clowd.Clipboard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;
using Lokql.Engine.Commands;
using lokqlDx;
using LokqlDx.Views;
using NotNullStrings;

namespace LokqlDx.ViewModels;

public partial class RenderingSurfaceViewModel : ObservableObject, IResultRenderingSurface
{
    private readonly IKustoConsole _console;
    private readonly KustoSettingsProvider _kustoSettings;

    [ObservableProperty] private bool _showData=true;
    [ObservableProperty] private string _dataGridSizeWarning = string.Empty;
    [ObservableProperty] private DisplayPreferencesViewModel _displayPreferences;

    [ObservableProperty] private MapViewModel _mapModel;

    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private ChartViewModel _chartModel;

    
    [ObservableProperty] private string _querySummary = string.Empty;
    [ObservableProperty] private KustoQueryResult _result = KustoQueryResult.Empty;
    [ObservableProperty] private bool _showDataGridSizeWarning;
    [ObservableProperty] private ITreeDataGridSource<Row> _treeSource = new MyFlatTreeDataGridSource<Row>([], []);

    public RenderingSurfaceViewModel(string name, KustoSettingsProvider kustoSettings,
        DisplayPreferencesViewModel displayPreferences, IKustoConsole console)
    {
        _kustoSettings = kustoSettings;
        _displayPreferences = displayPreferences;
        _console = console;
        _name = name;
        MapModel = new MapViewModel(kustoSettings);
        ChartModel = new ChartViewModel(kustoSettings);
    }

    public async Task RenderToDisplay(KustoQueryResult result)
    {
        Result = result;
        QuerySummary = $"{result.RowCount} rows in {(int)result.QueryDuration.TotalMilliseconds}ms";

        await RenderTable(result);
        ShowData = !result.IsChart;
        var ShowMap = result.IsChart && result.Visualization.ChartType == "scatterchart"
                                 && result.Visualization.PropertyOr("kind", "") == "map";
        ChartModel.Activate(!ShowMap);
        MapModel.Activate(ShowMap);
        if (ShowMap)
            MapModel.Render(result);
        else ChartModel.Render(result);
    }

    public byte[] RenderToImage(KustoQueryResult result, double pWidth, double pHeight)
        => ChartModel.RenderToImage(result, pWidth, pHeight);

    [RelayCommand]
    private async Task DataGridCopy(string extent)
    {
        if (OperatingSystem.IsWindows())
        {
            var source = TreeSource as MyFlatTreeDataGridSource<Row>;
            var cellSelection = source!.CellSelection;
            if (cellSelection is null) return;

            //we don't need any selection if we're going to copy the entire table
            if (extent == "table")
            {
                var txt = Result.EnumerateRows().Select(row => row.Select(ObjectToString).JoinString())
                    .JoinAsLines();
                await ClipboardAvalonia.SetTextAsync(txt);
                return;
            }

            //for rows, columns and cells, we do need a valid selection

            if (cellSelection.SelectedIndex.RowIndex.Count == 0)
                return;
            var (rowIndex, columnIndex) =
                (cellSelection.SelectedIndex.RowIndex[0], cellSelection.SelectedIndex.ColumnIndex);

            if (rowIndex >= Result.RowCount) return;
            if (columnIndex >= Result.ColumnCount) return;

            switch (extent)
            {
                case "cell":
                {
                    var txt = ObjectToString(Result.Get(columnIndex, rowIndex));
                    await ClipboardAvalonia.SetTextAsync(txt);
                }
                    break;

                case "column":
                {
                    var colHdr = Result.ColumnDefinitions()[columnIndex];
                    var txt = Result.EnumerateColumnData(colHdr).Select(ObjectToString).JoinAsLines();
                    await ClipboardAvalonia.SetTextAsync(txt);
                }
                    break;

                case "row":
                {
                    var colHdr = Result.ColumnDefinitions()[columnIndex];
                    var txt = Result.GetRow(rowIndex).Select(ObjectToString).JoinString();
                    await ClipboardAvalonia.SetTextAsync(txt);
                }
                    break;

                case "csv":
                {
                    var maxAllowed = 10000;
                    if (Result.RowCount > maxAllowed)
                    {
                        _console.Warn(
                            $"Results are too large to copy to clipboard - only {maxAllowed} lines are allowed");
                        break;
                    }

                    var settings = _kustoSettings.Snapshot();
                    settings.Set(CsvSerializer.CsvSerializerSettings.SkipHeaderOnSave.Name, true);
                    using var stream = new MemoryStream();
                    var csvSerializer = CsvSerializer.Default(settings, _console);

                    await csvSerializer.SaveTable(stream, Result);

                    var csvText = Encoding.UTF8.GetString(stream.ToArray());
                    await ClipboardAvalonia.SetTextAsync(csvText.TrimEnd());
                    _console.Info("Results copied to clipboard");
                }
                    break;
            }
        }
    }


    public void CopyToClipboard()
    {
        if (MapModel.Show)
            MapModel.CopyToClipboard();
        if (ChartModel.Show)
            ChartModel.CopyToClipboard();
    }

    private Task RenderTable(KustoQueryResult result)
    {
        //ensure that if there are no results we clear the data grid
        if (result.Error.IsNotBlank())
        {
            var errorRows = new[] { new Row([result.Error]) };
            var columnList = new ColumnList<Row> { new TextColumn<Row, object?>("Error", r => r[0]) };
            TreeSource = new MyFlatTreeDataGridSource<Row>(errorRows, columnList);
            return Task.CompletedTask;
        }

        DispatcherHelper.SafeInvoke(() =>
        {
            const int defaultMax = 10000;
            const string settingName = "datagrid.maxrows";
            var maxRows = _kustoSettings.GetIntOr(settingName, defaultMax);

            if (result.RowCount > maxRows)
                DataGridSizeWarning =
                    $"Warning: Displaying only the first {maxRows} rows of {result.RowCount} rows.  Set {settingName} to see more";

            ShowDataGridSizeWarning = result.RowCount > maxRows;

            var rows = result.EnumerateRows(maxRows)
                .Select(CreateRow)
                .ToArray();

            var columnList = new ColumnList<Row>();
            columnList.AddRange(
                result.ColumnDefinitions()
                    .Select(col => new TextColumn<Row, object?>(col.Name, r => r[col.Index]))
            );
            var source = new MyFlatTreeDataGridSource<Row>(rows, columnList);
            source.Selection = new TreeDataGridCellSelectionModel<Row>(source);
            TreeSource = source;
        });

        return Task.CompletedTask;
    }

    private static Row CreateRow(object?[] rowItems) =>
        //var strings = rowItems.Select(i => i?.ToString() ?? "<null>").ToArray();
        new(rowItems);

   
    private static string ObjectToString(object? item)
        => item?.ToString() ?? "<null>";

    /// <summary>
    ///     Avalonia's DataGrid doesn't support binding to a DataTable so
    ///     create a fake row with indexer
    /// </summary>
    public class Row(object?[] rowItems)
    {
        public object? this[int index]
        {
            set => throw new InvalidOperationException();
            //it's possible to get transient mismatches during update so
            //just return a dummy value if the binding is temporarily invalid
            get => index >= rowItems.Length ? string.Empty : ObjectToString(rowItems[index]);
        }
    }

    [RelayCommand]
    public void ChangeTab(string sender)
    {
        //invert
        ShowData = (sender == "data");
    }
  }
