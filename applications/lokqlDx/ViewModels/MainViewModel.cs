using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Controls;
using Kusto.Language.Symbols;
using KustoLoco.Core.Evaluation.BuiltIns;
using Lokql.Engine;
using Lokql.Engine.Commands;
using LokqlDx.Models;
using LokqlDx.Services;
using lokqlDxComponents.Services;
using lokqlDxComponents.Services.Assets;
using Microsoft.Extensions.DependencyInjection;
using NotNullStrings;

namespace LokqlDx.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DialogService _dialogService;

    private readonly DisplayPreferencesViewModel _displayPreferences;
    private readonly DockFactory _factory;
    private readonly AssetFolderImageProvider _imageProvider;
    private readonly ILauncher _launcher;
    private readonly PreferencesManager _preferencesManager;
    private readonly RegistryOperations _registryOperations;
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageProvider _storage;
    private readonly ToolManager _toolManager;
    private readonly WorkspaceManager _workspaceManager;
    private Dictionary<FunctionSymbol, ScalarFunctionInfo> _additionalFunctions = [];
    private CommandProcessor _commandProcessor;
    [ObservableProperty] private ConsoleViewModel _consoleViewModel;

    [ObservableProperty] private Workspace _currentWorkspace = new();
    private InteractiveTableExplorer _explorer;

    private string _initWorkspacePath = string.Empty;

    [ObservableProperty] private bool _isDirty;

    [ObservableProperty] private IRootDock? _layout;

    private bool _pluginsLoaded;

    [ObservableProperty] private ObservableCollection<RecentWorkspace> _recentWorkspaces = [];
    [ObservableProperty] private bool _showUpdateInfo;
    [ObservableProperty] private string _tabStripPlacement = "Left";

    private bool _trueClose;
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
        IStorageProvider storage,
        ILauncher launcher,
        IServiceProvider serviceProvider,
        AssetFolderImageProvider imageProvider
    )
    {
        _imageProvider = imageProvider;
        _serviceProvider = serviceProvider;
        _displayPreferences = new DisplayPreferencesViewModel();

        ConsoleViewModel = new ConsoleViewModel(_displayPreferences);
        _toolManager = new ToolManager(_displayPreferences, ConsoleViewModel);
        _dialogService = dialogService;
        _preferencesManager = preferencesManager;
        _commandProcessor = commandProcessorFactory.GetCommandProcessor();
        _workspaceManager = workspaceManager;
        _registryOperations = registryOperations;
        _storage = storage;
        _launcher = launcher;


        _explorer = CreateExplorer();

        WeakReferenceMessenger.Default.Register<RunningQueryMessage>(this,
            (_, m) => { m.Reply(HandleQueryRunning(m)); });
        WeakReferenceMessenger.Default.Register<CreateDocumentRequest>(this,
            (_, m) => { m.Reply(CreateDoc(m)); });


        _factory = new DockFactory(_toolManager);
    }

    private QueryLibraryViewModel QueryLibrary => _toolManager.LibraryViewModel;

    private void ResetLayout()
    {
        var queries = QueryLibrary.Queries.ToArray();
        Layout = _factory.GetOrResetLayout();
      

        foreach (var query in queries.ToArray())
            _factory.AddDocument(query);
        _factory.ShowQuery(queries.First());
    }

    private QueryDocumentViewModel CreateDoc(CreateDocumentRequest msg)
    {
        var q = AddQuery(msg.Title, string.Empty, string.Empty, true);
        msg.Model = q;
        return q;
    }


    private async Task<bool> HandleQueryRunning(RunningQueryMessage message)
    {
        if (message.IsRunning) await SaveBeforeQuery();
        else Messaging.Send(new SchemaUpdateMessage(_explorer.GetSchema()));
        return false;
    }


    internal void SetInitWorkspacePath(string workspacePath) => _initWorkspacePath = workspacePath;


    private QueryDocumentViewModel AddQuery(string name, string content, string preQueryText, bool isVisible)
    {
        var doc = CreateQuery(name, content, preQueryText, isVisible);
        QueryLibrary.Add(doc);
        return doc;
    }

    private QueryDocumentViewModel CreateQuery(string name, string content, string preQueryText, bool isVisible)
    {
        var adapter = _serviceProvider.GetRequiredService<IntellisenseClientAdapter>();
        var renderingSurfaceViewModel =
            new RenderingSurfaceViewModel(name, _explorer.Settings, _displayPreferences, ConsoleViewModel);
        var sharedExplorer = _explorer.ShareWithNewSurface(renderingSurfaceViewModel);
        var queryEditorViewModel = new QueryEditorViewModel(sharedExplorer,
            ConsoleViewModel,
            _displayPreferences,
            content,
            preQueryText,
            adapter);

        var doc = new QueryDocumentViewModel(name,queryEditorViewModel, renderingSurfaceViewModel,_explorer.Settings) { IsVisible = isVisible };
        return doc;
    }

    [RelayCommand]
    private async Task ShowAbout()
    {
        await _dialogService.ShowMessageBox($"About", $"""
                                                  LokqlDX
                                                  Version: {UpgradeManager.GetCurrentVersion()}
                                                  (C) 2025 Neil MacMullen
                                                  """);
    }
    [RelayCommand]
    private async Task LoadData()
    {
        var files = await _dialogService.OpenDataFiles();
        foreach (var storageFile in files)
        {
            var path = storageFile.TryGetLocalPath()!;
            if (path.IsNotBlank())
                await Messaging.Send(new LoadFileMessage(path));
        }
    }

    [RelayCommand]
    private async Task SaveData()
    {
        var file = await _dialogService.SaveDataFiles();
        if (file == null)
            return;
        var path = file.TryGetLocalPath()!;
        if (path.IsNotBlank())
            await Messaging.Send(new SaveFileMessage(path));
    }


    [RelayCommand]
    private async Task RenameQuery(QueryDocumentViewModel model)
    {
        var text = new RenamableText(model.Title);
        await _dialogService.ShowRenameDialogs(text);
        model.Title = text.NewText;
    }

    [RelayCommand]
    private async Task Initialize()
    {
        await _registryOperations.AssociateFileType(true);
        _preferencesManager.RetrieveUiPreferencesFromDisk();
        _preferencesManager.EnsureDefaultFolderExists();
        ApplyUiPreferences();
        RebuildRecentFilesList();

        await LoadWorkspace(_initWorkspacePath);

        var newVersion = await UpgradeManager.UpdateAvailable();
        if (newVersion.IsNotBlank())
        {
            ShowUpdateInfo = true;
            UpdateInfo = $"New version available - {newVersion}";
        }

        await Task.Run(_imageProvider.Init);
    }

    [RelayCommand]
    private async Task Closing(WindowClosingEventArgs? cancelEventArgs)
    {
        if (_trueClose)
            return;
        cancelEventArgs!.Cancel = true;
        var userChoice = await OfferSaveOfCurrentWorkspace();
        if (userChoice == YesNoCancel.Cancel)
            return;
        if (userChoice is YesNoCancel.No or YesNoCancel.Complete)
        {
            _trueClose = true;
            var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            lifetime?.MainWindow?.Close();
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
        ApplyUiPreferences();
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
        ApplyUiPreferences();
        _preferencesManager.SaveUiPrefs();
    }

    [RelayCommand]
    private void ToggleWordWrap()
    {
        _preferencesManager.UIPreferences.WordWrap = !_preferencesManager.UIPreferences.WordWrap;
        ApplyUiPreferences();
        _preferencesManager.SaveUiPrefs();
    }

    [RelayCommand]
    private void ToggleLineNumbers()
    {
        _preferencesManager.UIPreferences.ShowLineNumbers = !_preferencesManager.UIPreferences.ShowLineNumbers;
        ApplyUiPreferences();
        _preferencesManager.SaveUiPrefs();
    }

    [RelayCommand]
    private async Task RegisterFileAssociation() => await _registryOperations.AssociateFileType(true);

    [RelayCommand]
    private async Task NavigateToWiki(string path) => await _dialogService.ShowHelp(path);

    [RelayCommand]
    private async Task NavigateToUri(string path) => await _launcher.LaunchUriAsync(new Uri(path));

    private void ApplyUiPreferences()
    {
        var uiPreferences = _preferencesManager.UIPreferences;
        _displayPreferences.FontFamily = uiPreferences.FontFamily;
        _displayPreferences.FontSize = uiPreferences.FontSize;
        _displayPreferences.WordWrap = uiPreferences.WordWrap;
        _displayPreferences.ShowLineNumbers = uiPreferences.ShowLineNumbers;
        var theme = uiPreferences.Theme;
        ApplicationHelper.SetTheme(theme);
    }

    private InteractiveTableExplorer CreateExplorer() =>
        new(
            ConsoleViewModel,
            _workspaceManager.Settings,
            _commandProcessor,
            new NullResultRenderingSurface(),
            _additionalFunctions);

    private async Task LoadWorkspace(string path)
    {
        if (await OfferSaveOfCurrentWorkspace() == YesNoCancel.Cancel)
            return;

        RemoveAllDocs();
        //make sure we have the most recent global preferences
        var appPrefs = _preferencesManager.FetchApplicationPreferencesFromDisk();
        if (!_pluginsLoaded)
        {
            var pluginsFolder = appPrefs.PluginsFolder;
            if (pluginsFolder.IsNotBlank())
            {
                _commandProcessor =
                    PluginHelper.LoadCommands(pluginsFolder, _explorer._outputConsole, _commandProcessor);
                _additionalFunctions = PluginHelper.LoadKqlFunctions(pluginsFolder, _explorer._outputConsole);
            }

            _pluginsLoaded = true;
        }

        _explorer = CreateExplorer();
        _workspaceManager.Load(path);
        CurrentWorkspace = _workspaceManager.Workspace;

        //reset the list of queries
        QueryLibrary.Clear();

        // AsyncRelayCommand<T> has an IsRunning property
        await _explorer.RunInput(appPrefs.StartupScript);
        await _explorer.RunInput(_workspaceManager.Workspace.StartupScript);

        if (CurrentWorkspace.Queries.Any())
            foreach (var p in CurrentWorkspace.Queries)
                AddQuery(p.Name.NullToEmpty(), p.Text.NullToEmpty(), p.PreQueryText.NullToEmpty(), !p.IsHidden);
        else
            AddQuery("query", CurrentWorkspace.Text, string.Empty, true);
        ResetLayout();
        UpdateUiFromWorkspace();
        if (!appPrefs.HasShownLanding)
        {
            //NavigateToLanding();
            appPrefs.HasShownLanding = true;
            _preferencesManager.Save(appPrefs);
        }

        Messaging.Send(new SchemaUpdateMessage(_explorer.GetSchema()));
    }

    private void RemoveAllDocs()
    {
      _factory.RemoveAllDocuments();
      QueryLibrary.Clear();
    }

    /// <summary>
    ///     Update the UI because a new workspace has been loaded
    /// </summary>
    private void UpdateUiFromWorkspace()
    {
        var version = UpgradeManager.GetCurrentVersion();
        var title = _workspaceManager.Path.IsBlank()
            ? $"LokqlDX {version} - new workspace"
            : $"LokqlDX {version} - {Path.GetFileNameWithoutExtension(_workspaceManager.Path)} ({Path.GetDirectoryName(_workspaceManager.Path)})";

        WindowTitle = title;
    }

    private bool RecheckDirty()
    {
        IsDirty = QueryLibrary.IsDirty();
        return IsDirty;
    }

    private void ResetDirty() => QueryLibrary.ClearDirty();


    /// <summary>
    ///     Allow the user to save any pending changes
    /// </summary>
    /// <returns>
    ///     COMPLETE if the user did the save or didn't need to
    /// </returns>
    private async Task<YesNoCancel> OfferSaveOfCurrentWorkspace()
    {
        if (!RecheckDirty())
            return YesNoCancel.Complete;

        var shouldSave = _preferencesManager.FetchCachedApplicationSettings().AutoSave;
        //always show the dialog if this is a new workspace
        //because otherwise it's a little disconcerting to click close
        //and immediately find yourself in the save-as dialog
        if (!shouldSave || _workspaceManager.IsNewWorkspace || IsDirty)
        {
            var result = await _dialogService.ShowConfirmCancelBox("Warning",
                "You have have unsaved changes. Do you want to save them?");
            if (result == YesNoCancel.Cancel)
                return YesNoCancel.Cancel;
            if (result == YesNoCancel.No)
                ResetDirty();

            shouldSave = result == YesNoCancel.Yes;
        }

        if (shouldSave)
            return await Save();

        return YesNoCancel.Complete;
    }

    /// <summary>
    ///     Save the current workspace to the current file
    /// </summary>
    private async Task<YesNoCancel> Save()
    {
        if (!RecheckDirty())
            return YesNoCancel.Complete;

        if (_workspaceManager.IsNewWorkspace)
            return await SaveAs();

        SaveWorkspace(_workspaceManager.Path);
        return YesNoCancel.Complete;
    }

    private async Task SaveBeforeQuery()
    {
        if (!_preferencesManager.FetchCachedApplicationSettings().AutoSave)
            return;
        if (_workspaceManager.IsNewWorkspace)
            return;

        await Save();
    }

    /// <summary>
    ///     Save the current workspace to a new file
    /// </summary>
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


        if (result?.TryGetLocalPath() is { } path)
        {
            SaveWorkspace(path);
            //make sure we update title bar
            var title =
                $"{Path.GetFileNameWithoutExtension(_workspaceManager.Path)} ({Path.GetDirectoryName(_workspaceManager.Path)})";
            WindowTitle = title;
            return YesNoCancel.Complete;
        }

        //not saving the file counts as a cancel rather than "won't do anything"
        return YesNoCancel.Cancel;
    }

    private void SaveWorkspace(string path)
    {
        var queries = QueryLibrary.Persist();
        CurrentWorkspace.Queries = queries;
        _workspaceManager.Save(path, CurrentWorkspace);
        ResetDirty();
        UpdateMostRecentlyUsed(path);
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

    private void PersistUiPreferencesToDisk()
    {
        var ui = _preferencesManager.UIPreferences;
        ui.WindowLeft = WindowPosition.X;
        ui.WindowTop = WindowPosition.Y;
        ui.WindowWidth = WindowSize.Width;
        ui.WindowHeight = WindowSize.Height;
        _preferencesManager.SaveUiPrefs();
    }

    [RelayCommand]
    private void ShowTool(string toolName) => Messaging.Send(new ShowToolMessage(toolName));
}

public static class ApplicationHelper
{
    public static void SetTheme(string theme)
    {
        theme = theme.OrWhenBlank("Dark");
        Application.Current!.RequestedThemeVariant = theme switch
        {
            "Default" => ThemeVariant.Default,
            "Dark" => ThemeVariant.Dark,
            "Light" => ThemeVariant.Light,
            _ => ThemeVariant.Dark
        };
        Messaging.Send(new ThemeChangedMessage(theme));
    }

    public static string GetTheme() => Application.Current?.ActualThemeVariant.ToString().OrWhenBlank("Dark")!;

    public static IBrush GetBackgroundForCurrentTheme()
    {
        var theme = GetTheme();
        if (theme.ToLower() == "dark")
            return Brushes.Black;
        return Brushes.White;
    }
}
