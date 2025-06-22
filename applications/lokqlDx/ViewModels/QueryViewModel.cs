using CommunityToolkit.Mvvm.ComponentModel;
using Intellisense;
using Lokql.Engine;
using Microsoft.Extensions.Logging;

namespace LokqlDx.ViewModels;

public partial class QueryViewModel : ObservableObject
{
    [ObservableProperty] private QueryEditorViewModel _queryEditorViewModel;
    [ObservableProperty] private RenderingSurfaceViewModel _renderingSurfaceViewModel;
    [ObservableProperty] CopilotChatViewModel _copilotChatViewModel;
    private readonly InteractiveTableExplorer _explorer;

    public QueryViewModel(ConsoleViewModel console,InteractiveTableExplorer explorer,IntellisenseClient intellisenseClient,
        ILogger<QueryEditorViewModel> logger,string initialText,DisplayPreferencesViewModel displayPreferences)
    {
        RenderingSurfaceViewModel = new RenderingSurfaceViewModel(explorer.Settings,displayPreferences);
        _explorer = explorer.ShareWithNewSurface(RenderingSurfaceViewModel);
        CopilotChatViewModel = new CopilotChatViewModel();
       
        QueryEditorViewModel = new QueryEditorViewModel(_explorer,console,intellisenseClient,logger,displayPreferences,initialText);
       
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
