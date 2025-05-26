using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using Lokql.Engine.Commands;

namespace LokqlDx.ViewModels;

public partial class RenderingSurfaceViewModel : ObservableObject, IResultRenderingSurface
{
    [ObservableProperty] private List<string> _columns = [];

    [ObservableProperty] private string? _dataGridSizeWarning;
    [ObservableProperty] private double _fontSize = 20;
    private readonly KustoSettingsProvider _kustoSettings;
    [ObservableProperty] private ObservableCollection<Row>? _results;
    [ObservableProperty] private bool _showDataGridSizeWarning;

    public RenderingSurfaceViewModel(KustoSettingsProvider kustoSettings)
    {
        _kustoSettings = kustoSettings;
    }

    public async Task RenderToDisplay(KustoQueryResult result) => await RenderTable(result);

    public byte[] RenderToImage(KustoQueryResult result, double pWidth, double pHeight) => [];

    private Task RenderTable(KustoQueryResult result)
    {
        //ensure that if there are no results we clear the data grid
        if (result.RowCount == 0)
        {
            Results = null;
            return Task.CompletedTask;
        }

        const int defaultMax = 10000;
        const string settingName = "datagrid.maxrows";
        var maxRows = _kustoSettings.GetIntOr(settingName, defaultMax);

        if (result.RowCount > maxRows)
            DataGridSizeWarning =
                $"Warning: Displaying only the first {maxRows} rows of {result.RowCount} rows.  Set {settingName} to see more";

        ShowDataGridSizeWarning = result.RowCount > maxRows;
        var dt = result.ToDataTable(maxRows);

        var columns = new List<string>();
        //prevent the column names from being interpreted as hotkeys
        for (var i = 0; i < dt.Columns.Count; i++)
        {
            var c = dt.Columns[i];

            columns.Add(c.ColumnName);
        }

        var results = new ObservableCollection<Row>();

        foreach (DataRow row in dt.Rows)
        {
            var newRow = new Row();

            for (var i = 0; i < dt.Columns.Count; i++)
            {
                var c = dt.Columns[i];
                newRow[i] = row.ItemArray[i] ?? "";
            }

            results.Add(newRow);
        }

        Results = results;
        Columns = columns;

        return Task.CompletedTask;
    }

    internal void SetUiPreferences(UIPreferences uiPreferences) => FontSize = uiPreferences.FontSize;

    internal void Clear() => Results?.Clear();

    /// <summary>
    ///     This thing is stupid.<br />
    ///     Avalonia's DataGrid doesn't support binding to a DataTable.<br />
    ///     Untill someone comes up with a better idea, I'm converting DataTable to list of this abominations, and creating
    ///     rows in CodeBehind, it works for now
    /// </summary>
    public class Row
    {
        private Row? _next;

        public object? this[int index]
        {
            set
            {
                if (index < 10)
                    Prop(index)?.SetValue(this, value);
                else
                    Next[index - 10] = value;
            }
            get => index switch
            {
                < 10 => Prop(index)?.GetValue(this),
                _ => Next[index - 10]
            };
        }

        public object? Value0 { get; set; }
        public object? Value1 { get; set; }
        public object? Value2 { get; set; }
        public object? Value3 { get; set; }
        public object? Value4 { get; set; }
        public object? Value5 { get; set; }
        public object? Value6 { get; set; }
        public object? Value7 { get; set; }
        public object? Value8 { get; set; }
        public object? Value9 { get; set; }

        public Row Next
        {
            get
            {
                if (_next is null)
                    _next = new Row();

                return _next;
            }
        }

        public static string GetPath(int i)
        {
            if (i < 10) return $"Value{i}";

            return $"{nameof(Next)}.{GetPath(i - 10)}";
        }

        private static PropertyInfo? Prop(int index) => typeof(Row).GetProperty($"Value{index}");
    }
}
