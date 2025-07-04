using CommunityToolkit.Mvvm.ComponentModel;

namespace LokqlDx.ViewModels;

public partial class QueryViewModel : ObservableObject
{
    [ObservableProperty] private QueryEditorViewModel _queryEditorViewModel;
    [ObservableProperty] private RenderingSurfaceViewModel _renderingSurfaceViewModel;
    [ObservableProperty] private CopilotChatViewModel _copilotChatViewModel;

    public QueryViewModel(
        QueryEditorViewModel queryEditorViewModel,
        RenderingSurfaceViewModel renderingSurfaceViewModel,
        CopilotChatViewModel copilotChatViewModel)
    {
        QueryEditorViewModel = queryEditorViewModel;
        RenderingSurfaceViewModel = renderingSurfaceViewModel;
        CopilotChatViewModel = copilotChatViewModel;
    }

    public bool IsDirty()
    {
        return QueryEditorViewModel.IsDirty;
    }

    public string GetText()
    {
        return QueryEditorViewModel.GetText();
    }
}
