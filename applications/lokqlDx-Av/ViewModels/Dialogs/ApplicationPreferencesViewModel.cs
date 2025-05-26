using Avalonia.Media;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokqlDx.ViewModels.Dialogs;

public partial class ApplicationPreferencesViewModel : ObservableObject, IDialogViewModel
{
    private readonly ApplicationPreferences _applicationPreferences;
    private readonly PreferencesManager _preferencesManager;
    private readonly TaskCompletionSource _completionSource;
    public Task Result { get; private set; }

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

    [ObservableProperty] ObservableCollection<FontFamily> _fonts;
    [ObservableProperty] FontFamily _selectedFont;
    [ObservableProperty] double _fontSize;
    [ObservableProperty] bool _wordWrap;
    [ObservableProperty] bool _showLineNumbers;
    [ObservableProperty] bool _saveBeforeQuery;
    [ObservableProperty] TextDocument _document = new();

    [RelayCommand] void Save()
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

    [RelayCommand] void Cancel()
    {
        _completionSource.SetResult();
    }
}
