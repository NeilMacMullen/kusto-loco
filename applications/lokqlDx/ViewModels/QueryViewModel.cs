using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace LokqlDx.ViewModels;

public partial class QueryViewModel : ObservableObject
{
    [ObservableProperty] private int _chartColumn = 2;
    [ObservableProperty] private int _chartRow;
    [ObservableProperty] private int _columnSpan = 1;

    // Add observable properties for grid positions
    [ObservableProperty] private int _editorViewColumn;
    [ObservableProperty] private int _editorViewRow;
    [ObservableProperty] private int _gridSplitterColumn = 1;
    [ObservableProperty] private int _gridSplitterHeight = int.MaxValue;
    [ObservableProperty] private int _gridSplitterRow;
    [ObservableProperty] private int _gridSplitterWidth = 5;
    [ObservableProperty] private QueryEditorViewModel _queryEditorViewModel;
    [ObservableProperty] private RenderingSurfaceViewModel _renderingSurfaceViewModel;

    [ObservableProperty] private int _rowSpan = 3;
    private int n;

    public QueryViewModel(
        QueryEditorViewModel queryEditorViewModel,
        RenderingSurfaceViewModel renderingSurfaceViewModel)
    {
        QueryEditorViewModel = queryEditorViewModel;
        RenderingSurfaceViewModel = renderingSurfaceViewModel;
        Messaging.RegisterForEvent<LayoutChangedMessage>(this, FlipArrangement);
        Messaging.RegisterForEvent<CopyChartMessage>(this, CopyChartHandler);
        WeakReferenceMessenger.Default.Register<LoadFileMessage>(this,
            (_, m) =>
            {
                if (IsActive)
                    m.Reply(LoadFile(m));
            });
        WeakReferenceMessenger.Default.Register<SaveFileMessage>(this,
            (_, m) =>
            {
                if (IsActive)
                    m.Reply(SaveFile(m));
            });
    }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    private void CopyChartHandler()
    {
        if (IsActive)
            CopyChartToClipboard();
    }

    private void CopyChartToClipboard() => RenderingSurfaceViewModel.CopyToClipboard();

    private async Task LoadFile(LoadFileMessage msg)
    {
        await QueryEditorViewModel.RunQueryString($".load {msg.Path}");
    }

    private async Task SaveFile(SaveFileMessage msg) => await QueryEditorViewModel.RunQueryString($".save {msg.Path}");

    /// <summary>
    ///     Arrange the view so that items are stacked in rows (vertically).
    /// </summary>
    public void ArrangeInRows(bool editorFirst)
    {
        RowSpan = 1;
        ColumnSpan = 3;

        EditorViewColumn = 0;
        EditorViewRow = editorFirst ? 0 : 2;

        GridSplitterColumn = 0;
        GridSplitterRow = 1;
        GridSplitterHeight = 5;
        GridSplitterWidth = int.MaxValue;


        ChartColumn = 0;
        ChartRow = editorFirst ? 2 : 0;
    }

    /// <summary>
    ///     Arrange the view so that items are arranged in columns (horizontally).
    /// </summary>
    public void ArrangeInColumns(bool editorFirst)
    {
        RowSpan = 3;
        ColumnSpan = 1;

        EditorViewColumn = editorFirst ? 0 : 2;
        EditorViewRow = 0;

        GridSplitterColumn = 1;
        GridSplitterRow = 0;
        GridSplitterWidth = 5;
        GridSplitterHeight = int.MaxValue;

        ChartColumn = editorFirst ? 2 : 0;
        ChartRow = 0;
    }

    public bool IsDirty() => QueryEditorViewModel.IsDirty;

    public string GetText() => QueryEditorViewModel.GetText();

    [RelayCommand]
    private void FlipArrangement()
    {
        n = (n + 1) % 4;
        if (n == 0)
            ArrangeInColumns(true);
        if (n == 1)
            ArrangeInRows(true);
        if (n == 2)
            ArrangeInColumns(false);
        if (n == 3)
            ArrangeInRows(false);
    }


    public void Clean() => QueryEditorViewModel.IsDirty = false;

    [RelayCommand]
    public void CopyChart()
    {
        CopyChartToClipboard();
    }

    [RelayCommand]
    public void PinChart()
    {
        var msg = new PinResultMessage(new QueryResultWithSender(Name, RenderingSurfaceViewModel.Result, false));
        Messaging.Send(msg);
    }

    [RelayCommand]
    public void TearOff()
    {
        var msg = new PinResultMessage(new QueryResultWithSender(Name, RenderingSurfaceViewModel.Result, true));
        Messaging.Send(msg);
    }
}
