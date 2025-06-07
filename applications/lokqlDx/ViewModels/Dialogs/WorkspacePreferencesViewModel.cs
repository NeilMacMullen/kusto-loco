using Avalonia.Media;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LokqlDx.ViewModels.Dialogs;

public partial class WorkspacePreferencesViewModel : ObservableObject, IDialogViewModel
{
    private readonly TaskCompletionSource _completionSource;
    private readonly UIPreferences _uiPreferences;
    private readonly WorkspaceManager _workspaceManager;

    [ObservableProperty] private TextDocument _document = new();
    [ObservableProperty] private double _fontSize;
    [ObservableProperty] private FontFamily _selectedFont;
    [ObservableProperty] private bool _showLineNumbers;
    [ObservableProperty] private bool _wordWrap;

    public WorkspacePreferencesViewModel(WorkspaceManager workspaceManager, UIPreferences uiPreferences)
    {
        _workspaceManager = workspaceManager;
        _uiPreferences = uiPreferences;
        Document.Text = _workspaceManager.Workspace.StartupScript;

        var fonts = FontManager.Current.SystemFonts.OrderBy(x => x.Name);
        SelectedFont = fonts.First(x => x.Name == uiPreferences.FontFamily);
        FontSize = uiPreferences.FontSize;
        WordWrap = uiPreferences.WordWrap;
        ShowLineNumbers = uiPreferences.ShowLineNumbers;

        _completionSource = new TaskCompletionSource();
        Result = _completionSource.Task;
    }

    public Task Result { get; }

    [RelayCommand]
    private void Save()
    {
        _workspaceManager.Workspace.StartupScript = Document.Text;

        if (!_workspaceManager.IsNewWorkspace)
            _workspaceManager.Save(_workspaceManager.Path, _workspaceManager.Workspace);

        _completionSource.SetResult();
    }

    [RelayCommand]
    private void Cancel() => _completionSource.SetResult();
}
