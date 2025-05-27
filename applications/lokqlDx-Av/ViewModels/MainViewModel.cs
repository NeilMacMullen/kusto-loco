using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using Lokql.Engine.Commands;
using LokqlDx.Desktop;
using LokqlDx.Models;
using LokqlDx.Services;
using NotNullStrings;

namespace LokqlDx.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly BrowserServices _browserServices;
    private readonly CommandProcessorFactory _commandProcessorFactory;
    private readonly DialogService _dialogService;
    private readonly KustoSettingsProvider _kustoSettings;
    private readonly StandardFormatAdaptor _loader;
    private readonly PreferencesManager _preferencesManager;
    private readonly RegistryOperations _registryOperations;
    private readonly IStorageProvider _storage;
    private readonly WorkspaceManager _workspaceManager;

    [ObservableProperty] private ColumnDefinitions? _columnDefinitions
        = ColumnDefinitions.Parse("*,auto,*");

    private CommandProcessor _commandProcessor;
    [ObservableProperty] private ConsoleViewModel _consoleViewModel;
    [ObservableProperty] private CopilotChatViewModel _copilotChatViewModel;

    [ObservableProperty] private Workspace _currentWorkspace = new();

    private InteractiveTableExplorer _explorer;

    private string _initWorkspacePath = string.Empty;
    [ObservableProperty] private QueryEditorViewModel _queryEditorViewModel;
    [ObservableProperty] private ObservableCollection<RecentWorkspace> _recentWorkspaces = [];
    [ObservableProperty] private RenderingSurfaceViewModel _renderingSurfaceViewModel;
    [ObservableProperty] private RowDefinitions _rowDefinitions = RowDefinitions.Parse("*,auto,*");
    [ObservableProperty] private string _updateInfo = string.Empty;
    [ObservableProperty] private Point _windowPosition;
    [ObservableProperty] private Size _windowSize;
    [ObservableProperty] private string _windowTitle = "LokqlDX";

    public MainViewModel(
        DialogService dialogService,
        PreferencesManager preferencesManager,
        CommandProcessorFactory commandProcessorFactory,
        WorkspaceManager workspaceManager,
        RegistryOperations registryOperations,
        BrowserServices browserServices,
        IStorageProvider storage)
    {
        _dialogService = dialogService;
        _preferencesManager = preferencesManager;
        _commandProcessorFactory = commandProcessorFactory;
        _commandProcessor = _commandProcessorFactory.GetCommandProcessor();
        _workspaceManager = workspaceManager;
        _registryOperations = registryOperations;
        _browserServices = browserServices;
        _storage = storage;
        _kustoSettings = workspaceManager.Settings;

        ConsoleViewModel = new ConsoleViewModel();
        RenderingSurfaceViewModel = new RenderingSurfaceViewModel(_kustoSettings);
        CopilotChatViewModel = new CopilotChatViewModel();

        _loader = new StandardFormatAdaptor(_kustoSettings, ConsoleViewModel);
        _explorer = new InteractiveTableExplorer(
            ConsoleViewModel,
            _loader,
            _kustoSettings,
            _commandProcessor,
            RenderingSurfaceViewModel);

        QueryEditorViewModel = new QueryEditorViewModel(_explorer, ConsoleViewModel);
        QueryEditorViewModel.ExecutingQuery += QueryEditorViewModel_ExecutingQuery;
    }

    partial void OnCurrentWorkspaceChanged(Workspace value) => QueryEditorViewModel.CurrentWorkspace = value;

    internal void SetInitWorkspacePath(string workspacePath) => _initWorkspacePath = workspacePath;

    [RelayCommand]
    private async Task Initialize()
    {
        _preferencesManager.RetrieveUiPreferencesFromDisk();

        QueryEditorViewModel.AddInternalCommands(_commandProcessor.GetVerbs());
        await _registryOperations.AssociateFileType(true);
        _preferencesManager.EnsureDefaultFolderExists();
        ApplyUiPreferences(false);
        RebuildRecentFilesList();
        ResizeWindowAccordingToStoredPreferences();

        await LoadWorkspace(_initWorkspacePath);

        var isNewVersionAvailable = await UpgradeManager.UpdateAvailable();
        //if (isNewVersionAvailable)
        //{
        //    StatusBar.Visibility = Visibility.Visible;
        //    UpdateInfo.Content = "New version available";
        //}
    }


    [RelayCommand]
    private async Task Closing(WindowClosingEventArgs? cancelEventArgs)
    {
        if (cancelEventArgs is not null && await OfferSaveOfCurrentWorkspace() == YesNoCancel.Cancel)
        {
            cancelEventArgs.Cancel = true;
            return;
        }

        PersistUiPreferencesToDisk();
    }

    [RelayCommand]
    private async Task NewWorkspace()
    {
        if (await OfferSaveOfCurrentWorkspace() == YesNoCancel.Cancel)
            return;
        await LoadWorkspace(string.Empty);
    }

    [RelayCommand]
    private async Task OpenWorkspace()
    {
        var folder = _workspaceManager.ContainingFolder();

        var result = await _storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open workspace",
            FileTypeFilter =
            [
                new FilePickerFileType($"Lokql Workspace ({WorkspaceManager.GlobPattern})")
                {
                    Patterns = [WorkspaceManager.GlobPattern]
                }
            ],
            SuggestedFileName = Path.GetFileName(_workspaceManager.Path),
            SuggestedStartLocation = await _storage.TryGetFolderFromPathAsync(folder),
            AllowMultiple = false
        });

        if (result.Any())
        {
            var path = result[0].TryGetLocalPath().NullToEmpty();
            if (path.IsNotBlank())
                await LoadWorkspace(path);
        }
    }

    //This is a case where the parameter really _can_ be null since
    //the UI can call this even if the SelectedItem is null because
    //there are no items in the list
    private bool CanExecuteOpenRecentWorkspace(RecentWorkspace? obj) => obj != null;

    [RelayCommand(CanExecute = nameof(CanExecuteOpenRecentWorkspace))]
    private async Task OpenRecentWorkspace(RecentWorkspace workspace)
    {
        if (await OfferSaveOfCurrentWorkspace() == YesNoCancel.Cancel)
            return;
        await LoadWorkspace(workspace.Path);
    }

    [RelayCommand]
    private async Task SaveWorkspace() => await Save();

    [RelayCommand]
    private async Task SaveWorkspaceAs() => await SaveAs();

    [RelayCommand]
    private async Task OpenAppPreferences()
    {
        await _dialogService.ShowAppPreferences(_preferencesManager);
        ApplyUiPreferences(true);
    }

    [RelayCommand]
    private async Task OpenWorkspacePreferences() =>
        await _dialogService.ShowWorkspacePreferences(_workspaceManager, _preferencesManager.UIPreferences);

    [RelayCommand]
    private void ChangeFontSize(int by)
    {
        _preferencesManager.UIPreferences.FontSize = Math.Clamp(
            _preferencesManager.UIPreferences.FontSize + by,
            6, 40);
        ApplyUiPreferences(true);
    }

    [RelayCommand]
    private void ToggleWordWrap()
    {
        _preferencesManager.UIPreferences.WordWrap = !_preferencesManager.UIPreferences.WordWrap;
        ApplyUiPreferences(true);
    }

    [RelayCommand]
    private void ToggleLineNumbers()
    {
        _preferencesManager.UIPreferences.ShowLineNumbers = !_preferencesManager.UIPreferences.ShowLineNumbers;
        ApplyUiPreferences(true);
    }

    [RelayCommand]
    private async Task RegisterFileAssociation() => await _registryOperations.AssociateFileType(true);

    [RelayCommand]
    private async Task NavigateToWiki(string path) => await _dialogService.ShowHelp(path);

    [RelayCommand]
    private void NavigateToUri(string path) => _browserServices.OpenUriInBrowser(path);

    private void ApplyUiPreferences(bool skipGrid)
    {
        var uiPreferences = _preferencesManager.UIPreferences;
        QueryEditorViewModel.SetUiPreferences(uiPreferences);
        ConsoleViewModel.SetUiPreferences(uiPreferences);
        RenderingSurfaceViewModel.SetUiPreferences(uiPreferences);
        CopilotChatViewModel.SetUiPreferences(uiPreferences);

        if (skipGrid)
            return;
    }

    private async Task LoadWorkspace(string path)
    {
        if (await OfferSaveOfCurrentWorkspace() == YesNoCancel.Cancel)
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
        CurrentWorkspace = _workspaceManager.Workspace;

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
            : $"LokqlDX {version} - {Path.GetFileNameWithoutExtension(_workspaceManager.Path)} ({Path.GetDirectoryName(_workspaceManager.Path)})";

        WindowTitle = title;
        if (clearWorkingContext)
        {
            QueryEditorViewModel.SetText(CurrentWorkspace.Text);
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
        if (!_workspaceManager.IsDirty(CurrentWorkspace))
            return YesNoCancel.Yes;

        var shouldSave = _preferencesManager.FetchCachedApplicationSettings().AutoSave;
        //always show the dialog if this is a new workspace
        //because otherwise it's a little disconcerting to click close
        //and immediately find yourself in the save-as dialog
        if (!shouldSave || _workspaceManager.IsNewWorkspace)
        {
            var result = await _dialogService.ShowConfirmCancelBox("Warning",
                "You have have unsaved changes. Do you want to save them?");
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
        if (!_workspaceManager.IsDirty(CurrentWorkspace))
            return YesNoCancel.Yes;

        if (_workspaceManager.IsNewWorkspace) return await SaveAs();

        SaveWorkspace(_workspaceManager.Path);
        return YesNoCancel.Yes;
    }

    /// <summary>
    ///     Save the current workspace to a new file
    /// </summary
    /// <returns>
    ///     true if the user went ahead with the save
    /// </returns>
    private async Task<YesNoCancel> SaveAs()
    {
        var result = await _storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Workspace as...",
            FileTypeChoices =
            [
                new FilePickerFileType($"Lokql Workspace ({WorkspaceManager.GlobPattern})")
                {
                    Patterns = [WorkspaceManager.GlobPattern]
                }
            ],
            ShowOverwritePrompt = true,
            SuggestedFileName = Path.GetFileName(_workspaceManager.Path)
        });

        if (result is not null && result.TryGetLocalPath() is string path)
        {
            SaveWorkspace(path);
            //make sure we update title bar
            var version = UpgradeManager.GetCurrentVersion();
            var title =
                $"{Path.GetFileNameWithoutExtension(_workspaceManager.Path)} ({Path.GetDirectoryName(_workspaceManager.Path)})";
            WindowTitle = title;
            return YesNoCancel.Yes;
        }

        //not saving the file counts as a cancel rather than "won't do anything"
        return YesNoCancel.Cancel;
    }

    private void SaveWorkspace(string path)
    {
        _workspaceManager.Save(path, CurrentWorkspace!);
        UpdateMostRecentlyUsed(path);
    }

    private async Task QueryEditorViewModel_ExecutingQuery(object? sender, EventArgs args)
    {
        if (!_workspaceManager.IsNewWorkspace &&
            _preferencesManager.FetchCachedApplicationSettings().AutoSave)
            await Save();
    }

    private void UpdateMostRecentlyUsed(string path)
    {
        if (path.IsBlank())
            return;
        _preferencesManager.BringToTopOfMruList(path);

        //JumpList.AddToRecentCategory(path);

        RebuildRecentFilesList();
    }

    private void RebuildRecentFilesList()
    {
        RecentWorkspaces.Clear();
        foreach (var path in _preferencesManager.GetMruItems().Take(10))
            RecentWorkspaces.Add(new RecentWorkspace(
                $"{Path.GetFileName(path)} ({Path.GetDirectoryName(path)})",
                path));
    }

    private void ResizeWindowAccordingToStoredPreferences()
    {
        var ui = _preferencesManager.UIPreferences;
        WindowPosition = new Point(ui.WindowLeft, ui.WindowTop);
        WindowSize = new Size(ui.WindowWidth, ui.WindowHeight);
        var columnDefs = ColumnDefinitions.Parse(
            ui.AvGridRowSerialization.JoinString());
        if (columnDefs.Count == ColumnDefinitions.Count)
            ColumnDefinitions = columnDefs;

        var rowDefs = RowDefinitions.Parse(ui.AvGridRowSerialization.JoinString());
        if (rowDefs.Count == RowDefinitions.Count)
            RowDefinitions = rowDefs;
    }

    /// <summary>
    ///     Allows the grid layout to be reset in case it ends up with off-screen values
    /// </summary>
    [RelayCommand]
    private void ResetGridLayout()
    {
        ColumnDefinitions = ColumnDefinitions.Parse($"{WindowSize.Width * 0.6},auto,*");
        RowDefinitions = RowDefinitions.Parse($"{WindowSize.Height * 0.6},auto,*");
    }


    private void PersistUiPreferencesToDisk()
    {
        var ui = _preferencesManager.UIPreferences;
        ui.WindowLeft = WindowPosition.X;
        ui.WindowTop = WindowPosition.Y;
        ui.WindowWidth = WindowSize.Width;
        ui.WindowHeight = WindowSize.Height;
        ui.AvGridRowSerialization = ColumnDefinitions!.ToString().Tokenize(",");
        ui.AvGridColumnSerialization = RowDefinitions!.ToString().Tokenize(",");
        _preferencesManager.SaveUiPrefs();
    }
}
