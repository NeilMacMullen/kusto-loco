using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Clowd.Clipboard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using Lokql.Engine.Commands;
using lokqlDx;
using LokqlDx.Views;
using NotNullStrings;
using Vanara.Extensions;

namespace LokqlDx.ViewModels;

public partial class RenderingSurfaceViewModel : ObservableObject, IResultRenderingSurface
{
    private readonly KustoSettingsProvider _kustoSettings;

    [ObservableProperty] private int _activeTab;
  
    [ObservableProperty] private string _dataGridSizeWarning = string.Empty;
   


    private IScottPlotHost _plotter = new NullScottPlotHost();
    [ObservableProperty] private ITreeDataGridSource<Row> _treeSource = new MyFlatTreeDataGridSource<Row>([], []);
    [ObservableProperty] private bool _showDataGridSizeWarning;
    [ObservableProperty] private KustoQueryResult _result = KustoQueryResult.Empty;
    [ObservableProperty] private string _querySummary =string.Empty;
    [ObservableProperty] private DisplayPreferencesViewModel _displayPreferences;

    [RelayCommand]
    private void DatagridCopy(string extent)
    {
        if (OperatingSystem.IsWindows())
        {
            var source = TreeSource as MyFlatTreeDataGridSource<Row>;
            var cellSelection = source!.CellSelection;
            if (cellSelection is null) return;

            //we don't need any selection if we're going to copy the entire table
            if (extent == "table")
            {
                var txt = Result.EnumerateRows().Select(row => row.Select(ObjectToString).JoinString(","))
                    .JoinAsLines();
                ClipboardAvalonia.SetText(txt);
                return;
            }

            //for rows, columns and cells, we do need a valid selection
            
            if (cellSelection.SelectedIndex.RowIndex.Count == 0)
                return;
            var (rowIndex, columnIndex) = (cellSelection.SelectedIndex.RowIndex[0], cellSelection.SelectedIndex.ColumnIndex);

            if (rowIndex >= Result.RowCount) return;
            if (columnIndex >= Result.ColumnCount) return;

            switch (extent)
            {
                case "cell":
                {
                    var txt = ObjectToString(Result.Get(columnIndex, rowIndex));
                    ClipboardAvalonia.SetText(txt);
                }
                    break;

                case "column":
                {
                    var colHdr = Result.ColumnDefinitions()[columnIndex];
                    var txt =Result.EnumerateColumnData(colHdr).Select(ObjectToString).JoinAsLines();
                    ClipboardAvalonia.SetText(txt);
                }
                    break;

                case "row":
                {
                    var colHdr = Result.ColumnDefinitions()[columnIndex];
                    var txt = Result.GetRow(rowIndex).Select(ObjectToString).JoinString(",");
                    ClipboardAvalonia.SetText(txt);
                }
                    break;
                default:
                    break;
            }
        }
    }

    public RenderingSurfaceViewModel(KustoSettingsProvider kustoSettings,DisplayPreferencesViewModel displayPreferences)
    {
        _kustoSettings = kustoSettings;
        _displayPreferences = displayPreferences;
    }

    public async Task RenderToDisplay(KustoQueryResult result)
    {
        Result = result;
        QuerySummary = $"{result.RowCount} rows in {(int)result.QueryDuration.TotalMilliseconds}ms";
        
         await RenderTable(result);
        _plotter.RenderToDisplay(result, _kustoSettings);
        ActiveTab = result.IsChart
            ? 1 //show the plot tab
            : 0; //show the table tab
    }

    public byte[] RenderToImage(KustoQueryResult result, double pWidth, double pHeight)
        => _plotter.RenderToImage(result, pWidth, pHeight, _kustoSettings);

    private Task RenderTable(KustoQueryResult result)
    {
        //ensure that if there are no results we clear the data grid
        if (result.Error.IsNotBlank())
        {
            var errorRows = new[] {new Row([result.Error])};
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

            var rows = result.EnumerateRows()
                .Take(maxRows)
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

    private static Row CreateRow(object?[] rowItems)
    {
        //var strings = rowItems.Select(i => i?.ToString() ?? "<null>").ToArray();
        return new Row(rowItems);
    }

    public void RegisterHost(IScottPlotHost plotter)
    {
        _plotter = plotter;
    }

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

    private static string ObjectToString(object? item)
        => item?.ToString() ?? "<null>";
}
