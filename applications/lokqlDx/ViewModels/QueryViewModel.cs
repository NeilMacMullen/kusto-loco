using System.Diagnostics.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace LokqlDx.ViewModels;

public partial class QueryViewModel : ObservableObject
{
    [ObservableProperty] private QueryEditorViewModel _queryEditorViewModel;
    [ObservableProperty] private RenderingSurfaceViewModel _renderingSurfaceViewModel;
    [ObservableProperty] private CopilotChatViewModel _copilotChatViewModel;

    [ObservableProperty] private int _rowSpan = 3;
    [ObservableProperty] private int _columnSpan = 1;
    // Add observable properties for grid positions
    [ObservableProperty] private int _editorViewColumn=0;
    [ObservableProperty] private int _editorViewRow=0;
    [ObservableProperty] private int _gridSplitterRow=0;
    [ObservableProperty] private int _gridSplitterColumn=1;
    [ObservableProperty] private int _chartColumn=2;
    [ObservableProperty] private int _chartRow=0;
    [ObservableProperty] private int _gridSplitterWidth = 5;
    [ObservableProperty] private int _gridSplitterHeight = int.MaxValue;

    public QueryViewModel(
        QueryEditorViewModel queryEditorViewModel,
        RenderingSurfaceViewModel renderingSurfaceViewModel,
        CopilotChatViewModel copilotChatViewModel)
    {
        QueryEditorViewModel = queryEditorViewModel;
        RenderingSurfaceViewModel = renderingSurfaceViewModel;
        CopilotChatViewModel = copilotChatViewModel;
        WeakReferenceMessenger.Default.Register<LayoutChangedMessage>(this, (r, m) =>
        {
           FlipArrangement();
        });
    }

    /// <summary>
    /// Arrange the view so that items are stacked in rows (vertically).
    /// </summary>
    public void ArrangeInRows(bool editorFirst)
    {
        RowSpan = 1;
        ColumnSpan = 3;

        EditorViewColumn = 0;
        EditorViewRow = editorFirst ? 0:2;

        GridSplitterColumn = 0;
        GridSplitterRow = 1;
        GridSplitterHeight = 5;
        GridSplitterWidth = int.MaxValue;

            
        ChartColumn = 0;
        ChartRow = editorFirst ? 2 : 0;
    }

    /// <summary>
    /// Arrange the view so that items are arranged in columns (horizontally).
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

    public bool IsDirty()
    {
        return QueryEditorViewModel.IsDirty;
    }

    public string GetText()
    {
        return QueryEditorViewModel.GetText();
    }

    private int n = 1;
    [RelayCommand]
    private void FlipArrangement()
    {
        if (n==1)
            ArrangeInRows(true);
        if (n == 2)
            ArrangeInRows(false);
        if (n == 3)
            ArrangeInColumns(true);
        if (n == 0)
            ArrangeInColumns(false);
        n = (n + 1) % 4;
    }

   
}
