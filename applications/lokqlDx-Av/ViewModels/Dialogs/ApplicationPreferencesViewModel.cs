using System.Collections.ObjectModel;
using Avalonia.Media;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LokqlDx.ViewModels.Dialogs;

public partial class ApplicationPreferencesViewModel : ObservableObject, IDialogViewModel
{
    private readonly ApplicationPreferences _applicationPreferences;
    private readonly TaskCompletionSource _completionSource;
    private readonly PreferencesManager _preferencesManager;
    [ObservableProperty] private TextDocument _document = new();

    [ObservableProperty] private ObservableCollection<FontFamily> _fonts;
    [ObservableProperty] private double _fontSize;
    [ObservableProperty] private bool _saveBeforeQuery;
    [ObservableProperty] private FontFamily _selectedFont;
    [ObservableProperty] private bool _showLineNumbers;
    [ObservableProperty] private bool _wordWrap;

    public ApplicationPreferencesViewModel(PreferencesManager preferencesManager)
    {
        _preferencesManager = preferencesManager;
        var applicationPreferences = preferencesManager.FetchCachedApplicationSettings();
        var uiPreferences = preferencesManager.UIPreferences;
        _applicationPreferences = applicationPreferences;

        Fonts = [.. FontManager.Current.SystemFonts.OrderBy(x => x.Name)];
        SelectedFont = Fonts.First(x => x.Name == uiPreferences.FontFamily);
        FontSize = uiPreferences.FontSize;
        WordWrap = uiPreferences.WordWrap;
        ShowLineNumbers = uiPreferences.ShowLineNumbers;
        SaveBeforeQuery = applicationPreferences.AutoSave;

        Document.Text = applicationPreferences.StartupScript;

        _completionSource = new TaskCompletionSource();
        Result = _completionSource.Task;
    }

    public Task Result { get; }

    [RelayCommand]
    private void Save()
    {
        _applicationPreferences.StartupScript = Document.Text;
        _applicationPreferences.AutoSave = SaveBeforeQuery;

        _preferencesManager.UIPreferences.ShowLineNumbers = ShowLineNumbers;
        _preferencesManager.UIPreferences.WordWrap = WordWrap;
        _preferencesManager.UIPreferences.FontFamily = SelectedFont.Name;
        _preferencesManager.UIPreferences.FontSize = FontSize;

        _preferencesManager.Save(_applicationPreferences);
        _preferencesManager.SaveUiPrefs();

        _completionSource.SetResult();
    }

    [RelayCommand]
    private void Cancel() => _completionSource.SetResult();
}
