using Avalonia.Media;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokqlDx.ViewModels.Dialogs;
public partial class WorkspacePreferencesViewModel : ObservableObject, IDialogViewModel
{
    private readonly TaskCompletionSource _completionSource;
    private readonly WorkspaceManager _workspaceManager;
    private readonly UIPreferences _uiPreferences;
    public Task Result { get; private set; }

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

    [ObservableProperty] TextDocument _document = new();
    [ObservableProperty] FontFamily _selectedFont;
    [ObservableProperty] double _fontSize;
    [ObservableProperty] bool _wordWrap;
    [ObservableProperty] bool _showLineNumbers;

    [RelayCommand] void Save()
    {
        _workspaceManager.Workspace.StartupScript = Document.Text;

        if (!_workspaceManager.IsNewWorkspace)
            _workspaceManager.Save(_workspaceManager.Path, _workspaceManager.Workspace);

        _completionSource.SetResult();
    }

    [RelayCommand] void Cancel()
    {
        _completionSource.SetResult();
    }
}
