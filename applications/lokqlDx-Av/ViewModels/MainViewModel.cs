using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using Lokql.Engine.Commands;
using lokqlDx;
using LokqlDx.Desktop;
using LokqlDx.Services;
using LokqlDx.ViewModels;
using NotNullStrings;

namespace LokqlDx.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DialogService _dialogService;
    private readonly PreferencesManager _preferencesManager;
    private readonly CommandProcessorFactory _commandProcessorFactory;
    private readonly WorkspaceManager _workspaceManager;
    private readonly RegistryOperations _registryOperations;
    private readonly IStorageProvider _storage;
    private readonly KustoSettingsProvider _kustoSettings;
    private readonly StandardFormatAdaptor _loader;

    private InteractiveTableExplorer _explorer;
    private CommandProcessor _commandProcessor;
    private Workspace? _currentWorkspace;
    private string? _initWorkspacePath;

    public MainViewModel(
        DialogService dialogService,
        PreferencesManager preferencesManager,
        CommandProcessorFactory commandProcessorFactory,
        WorkspaceManager workspaceManager,
        RegistryOperations registryOperations,
        IStorageProvider storage)
    {
        _dialogService = dialogService;
        _preferencesManager = preferencesManager;
        _commandProcessorFactory = commandProcessorFactory;
        _commandProcessor = _commandProcessorFactory.GetCommandProcessor();
        _workspaceManager = workspaceManager;
        _registryOperations = registryOperations;
        _storage = storage;
        _kustoSettings = workspaceManager.Settings;

        ConsoleViewModel = new();
        RenderingSurfaceViewModel = new(_kustoSettings);
        CopilotChatViewModel = new();

        _loader = new StandardFormatAdaptor(_kustoSettings, ConsoleViewModel);
        _explorer = new InteractiveTableExplorer(
            ConsoleViewModel,
            _loader,
            _kustoSettings,
            _commandProcessor,
            RenderingSurfaceViewModel);

        QueryEditorViewModel = new(_explorer, ConsoleViewModel);
    }

    [ObservableProperty] string _windowTitle = "LokqlDX";
    [ObservableProperty] ConsoleViewModel _consoleViewModel;
    [ObservableProperty] RenderingSurfaceViewModel _renderingSurfaceViewModel;
    [ObservableProperty] QueryEditorViewModel _queryEditorViewModel;
    [ObservableProperty] CopilotChatViewModel _copilotChatViewModel;
    [ObservableProperty] ColumnDefinitions? _columnDefinitions;
    [ObservableProperty] RowDefinitions? _rowDefinitions;
    
    internal void SetInitWorkspacePath(string workspacePath)
    {
        _initWorkspacePath = workspacePath;
    }

    [RelayCommand] async Task Initialize()
    {
        _preferencesManager.RetrieveUiPreferencesFromDisk();

        QueryEditorViewModel.AddInternalCommands(_commandProcessor.GetVerbs());
        await _registryOperations.AssociateFileType(true);
        _preferencesManager.EnsureDefaultFolderExists();

        ApplyUiPreferences();
        //TODO: rebuild recents
        //TODO: resize window

        await LoadWorkspace(_initWorkspacePath ?? "");

        //var isNewVersionAvailable = await UpgradeManager.UpdateAvailable();
        //if (isNewVersionAvailable)
        //{
        //    StatusBar.Visibility = Visibility.Visible;
        //    UpdateInfo.Content = "New version available";
        //}
    }

    [RelayCommand] async Task OpenAppPreferences()
    {
        await _dialogService.ShowAppPreferences();
    }

    private void ApplyUiPreferences()
    {
        var uiPreferences = _preferencesManager.UIPreferences;
        QueryEditorViewModel.SetUiPreferences(uiPreferences);
        ConsoleViewModel.SetUiPreferences(uiPreferences);
        RenderingSurfaceViewModel.SetUiPreferences(uiPreferences);
        CopilotChatViewModel.SetUiPreferences(uiPreferences);

        var columnDefs = ColumnDefinitions.Parse(
            string.Join(",", uiPreferences.MainGridSerialization));
        ColumnDefinitions = columnDefs;

        var rowDefs = RowDefinitions.Parse(
            string.Join(",", uiPreferences.MainGridSerialization));
        RowDefinitions = rowDefs;
    }

    private async Task LoadWorkspace(string path)
    {
        if ((await OfferSaveOfCurrentWorkspace()) == YesNoCancel.Cancel)
            return;

        //create a new explorer context
        var loader = new StandardFormatAdaptor(
            _workspaceManager.Settings, ConsoleViewModel);

        _commandProcessor = _commandProcessorFactory.GetCommandProcessor();
        _explorer = new InteractiveTableExplorer(
            ConsoleViewModel,
            loader,
            _workspaceManager.Settings,
            _commandProcessor,
            RenderingSurfaceViewModel);

        //make sure we have the most recent global preferences
        var appPrefs = _preferencesManager.FetchApplicationPreferencesFromDisk();
        _workspaceManager.Load(path);
        _currentWorkspace = _workspaceManager.Workspace;

        // AsyncRelayCommand<T> has an IsRunning property
        await QueryEditorViewModel.RunQueryCommand.ExecuteAsync(appPrefs.StartupScript);
        await QueryEditorViewModel.RunQueryCommand.ExecuteAsync(_workspaceManager.Workspace.StartupScript);
        //UpdateMostRecentlyUsed(path);
        UpdateUIFromWorkspace(true);
        if (!appPrefs.HasShownLanding)
        {
            //NavigateToLanding();
            appPrefs.HasShownLanding = true;
            _preferencesManager.Save(appPrefs);
        }
    }

    /// <summary>
    ///     Update the UI because a new workspace has been loaded
    /// </summary>
    /// <remarks>
    ///     The clearWorkingContext flags indicates whether we should clear all working context
    ///     We don't always want to do this, for example if we are doing a save-as in which case it's
    ///     a bit disconcerting for the user if all their charts/tables disappear
    /// </remarks>
    private void UpdateUIFromWorkspace(bool clearWorkingContext)
    {
        var version = UpgradeManager.GetCurrentVersion();
        var title = _workspaceManager.Path.IsBlank()
            ? $"LokqlDX {version} - new workspace"
            : $"{Path.GetFileNameWithoutExtension(_workspaceManager.Path)} ({Path.GetDirectoryName(_workspaceManager.Path)})";

        WindowTitle = title;
        if (clearWorkingContext)
        {
            QueryEditorViewModel.SetText(_currentWorkspace?.Text ?? "");
            RenderingSurfaceViewModel.Clear();
        }
    }

    /// <summary>
    ///     Allow the user to save any pending changes
    /// </summary>
    /// <returns>
    ///     true if the user did the save or didn't need to
    /// </returns>
    private async Task<YesNoCancel> OfferSaveOfCurrentWorkspace()
    {
        if (!_workspaceManager.IsDirty(_currentWorkspace))
            return YesNoCancel.Yes;

        var shouldSave = _preferencesManager.FetchCachedApplicationSettings().AutoSave;
        //always show the dialog if this is a new workspace
        //because otherwise it's a little disconcerting to click close
        //and immediately find yourself in the save-as dialog
        if (!shouldSave || _workspaceManager.IsNewWorkspace)
        {
            var result = await _dialogService.ShowConfirmCancelBox("Warning", "You have have unsaved changes. Do you want to save them?");
            if (result == YesNoCancel.Cancel)
                return YesNoCancel.Cancel;

            shouldSave = result == YesNoCancel.Yes;
        }

        if (shouldSave)
            return await Save();

        return YesNoCancel.No;
    }

    /// <summary>
    ///     Save the current workspace to the current file
    /// </summary>
    private async Task<YesNoCancel> Save()
    {
        if (!_workspaceManager.IsDirty(_currentWorkspace))
            return YesNoCancel.Yes;

        if (_workspaceManager.IsNewWorkspace) return await SaveAs();

        SaveWorkspace(_workspaceManager.Path);
        return YesNoCancel.Yes;
    }

    /// <summary>
    ///     Save the current workspace to a new file
    /// </summary>
    /// <returns>
    ///     true if the user went ahead with the save
    /// </returns>
    private async Task<YesNoCancel> SaveAs()
    {
        var result = await _storage.SaveFilePickerAsync(new()
        {
            Title = "Save Workspace as...",
            FileTypeChoices = [
                new($"Lokql Workspace ({WorkspaceManager.GlobPattern})"){
                    Patterns = [WorkspaceManager.GlobPattern]
                }
            ],
            SuggestedFileName = Path.GetFileName(_workspaceManager.Path),
        });

        if (result is not null && result.TryGetLocalPath() is string path)
        {
            SaveWorkspace(path);
            //make sure we update title bar
            //UpdateUIFromWorkspace(false);
            return YesNoCancel.Yes;
        }

        //not saving the file counts as a cancel rather than "won't do anything"
        return YesNoCancel.Cancel;
    }

    private void SaveWorkspace(string path)
    {
        //UpdateCurrentWorkspaceFromUI();
        _workspaceManager.Save(path, _currentWorkspace!);
        //UpdateMostRecentlyUsed(path);
    }
}
