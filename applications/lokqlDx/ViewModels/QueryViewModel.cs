using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KustoLoco.Core.Settings;
using System.ComponentModel;
using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public partial class QueryDocumentViewModel : Document
{
    [ObservableProperty] public bool _editLocked = true;
    [ObservableProperty] private bool _isDeleted;

    [ObservableProperty] private bool _isVisible = true;


    private bool _initialized;
    private bool _titleDirty;

    private void ActiveDocumentChanged(QueryDocumentViewModel model) => IsThisModelActive = model == this;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (_initialized && e.PropertyName == nameof(Title))
            _titleDirty = true;
        base.OnPropertyChanged(e);
    }
    private readonly KustoSettingsProvider _settings;
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

    public QueryDocumentViewModel(string title,QueryEditorViewModel queryEditorViewModel,
        RenderingSurfaceViewModel renderingSurfaceViewModel,KustoSettingsProvider settings)
    {
        Title = title;
        Messaging.RegisterForValue<TabChangedMessage, QueryDocumentViewModel>(this, ActiveDocumentChanged);
        _initialized = true;
        _settings = settings;
        QueryEditorViewModel = queryEditorViewModel;
        RenderingSurfaceViewModel = renderingSurfaceViewModel;
        WeakReferenceMessenger.Default.Register<LoadFileMessage>(this,
            (_, m) =>
            {
                if (IsThisModelActive)
                    m.Reply(LoadFile(m));
            });
        WeakReferenceMessenger.Default.Register<SaveFileMessage>(this,
            (_, m) =>
            {
                if (IsThisModelActive)
                    m.Reply(SaveFile(m));
            });
    }

    public bool IsThisModelActive { get; set; }


    private void CopyChartToClipboard() => RenderingSurfaceViewModel.CopyToClipboard();

    private async Task LoadFile(LoadFileMessage msg) => await QueryEditorViewModel.RunQueryString($".load {msg.Path}");

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

    public bool IsDirty() => QueryEditorViewModel.IsDirty() || _titleDirty;

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


    public void Clean()
    {
        QueryEditorViewModel.Clean();
        _titleDirty=false;
    }

    [RelayCommand]
    public void CopyChart() => CopyChartToClipboard();

   

    [RelayCommand]
    public void PinChart()
    {
        var msg = new PinResultMessage(new QueryResultWithSender(Title, RenderingSurfaceViewModel.Result,_settings,false));
        Messaging.Send(msg);
    }

    [RelayCommand]
    public void TearOff()
    {
        var msg = new PinResultMessage(new QueryResultWithSender(Title, RenderingSurfaceViewModel.Result,
            _settings,true));
        Messaging.Send(msg);
    }

    [RelayCommand]
    public void ToggleCursor()
    {
        Messaging.Send(new ToggleCursorMessage(""));
    }
    public string GetPreQueryText() => QueryEditorViewModel.QueryContextViewModel.Text;
}
