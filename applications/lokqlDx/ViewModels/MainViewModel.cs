using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Lokql.Engine;
using Lokql.Engine.Commands;
using LokqlDx.Desktop;
using LokqlDx.Models;
using LokqlDx.Services;
using lokqlDxComponents.Events;
using lokqlDxComponents.Services;
using NotNullStrings;

namespace LokqlDx.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string NewQueryName = "new";
    private readonly CommandProcessor _commandProcessor;
    private readonly CommandProcessorFactory _commandProcessorFactory;
    private readonly DialogService _dialogService;

    private readonly DisplayPreferencesViewModel _displayPreferences;
    private readonly ILauncher _launcher;
    private readonly PreferencesManager _preferencesManager;
    private readonly RegistryOperations _registryOperations;
    private readonly IStorageProvider _storage;
    private readonly WorkspaceManager _workspaceManager;
    [ObservableProperty] private int _activeQueryIndex;
    [ObservableProperty] private ConsoleViewModel _consoleViewModel;

    [ObservableProperty] private Workspace _currentWorkspace = new();

    private InteractiveTableExplorer _explorer;

    private string _initWorkspacePath = string.Empty;

    [ObservableProperty] private bool _isDirty;

    [ObservableProperty] private ObservableCollection<QueryItemViewModel> _queries = new();
    [ObservableProperty] private ObservableCollection<RecentWorkspace> _recentWorkspaces = [];
    [ObservableProperty] private bool _showUpdateInfo;
    [ObservableProperty] private string _tabStripPlacement = "Left";
    [ObservableProperty] private string _updateInfo = string.Empty;
    [ObservableProperty] private Point _windowPosition;
    [ObservableProperty] private Size _windowSize;
    [ObservableProperty] private string _windowTitle = "LokqlDX";
    private readonly AssetFolderImageProvider _imageProvider;
    private readonly QueryEditorScopedContextFactory _queryEditorScopedContextFactory;


    public MainViewModel(
        DialogService dialogService,
        PreferencesManager preferencesManager,
        CommandProcessorFactory commandProcessorFactory,
        WorkspaceManager workspaceManager,
        RegistryOperations registryOperations,
        IStorageProvider storage,
        ILauncher launcher,
        AssetFolderImageProvider imageProvider,
        QueryEditorScopedContextFactory queryEditorScopedContextFactory
    )
    {
        _queryEditorScopedContextFactory = queryEditorScopedContextFactory;
        _imageProvider = imageProvider;
        _displayPreferences = new DisplayPreferencesViewModel();
        _dialogService = dialogService;
        _preferencesManager = preferencesManager;
        _commandProcessorFactory = commandProcessorFactory;
        _commandProcessor = _commandProcessorFactory.GetCommandProcessor();
        _workspaceManager = workspaceManager;
        _registryOperations = registryOperations;
        _storage = storage;
        _launcher = launcher;
        var kustoSettings = workspaceManager.Settings;

        ConsoleViewModel = new ConsoleViewModel(_displayPreferences);
        //create a new explorer context
        var loader = new StandardFormatAdaptor(
            _workspaceManager.Settings, ConsoleViewModel);


        _explorer = CreateExplorer();
        // Register a message in some module
        WeakReferenceMessenger.Default.Register<RunningQueryMessage>(this,
            (r, m) => { m.Reply(HandleQueryRunning(m)); });
    }

    private async Task<bool> HandleQueryRunning(RunningQueryMessage message)
    {
        if (message.IsRunning) await SaveBeforeQuery();

        return false;
    }

    partial void OnCurrentWorkspaceChanged(Workspace value)
    {
        //QueryEditorViewModel.CurrentWorkspace = value;
    }

    internal void SetInitWorkspacePath(string workspacePath) => _initWorkspacePath = workspacePath;

    [RelayCommand]
    private void AddQuery() => AddQuery(NewQueryName, string.Empty);

    private void AddQuery(string name, string content)
        => AddQuery(name, content, Queries.Count);

    private void AddQuery(string name, string content, int desiredIndex)
    {

        var renderingSurfaceViewModel = new RenderingSurfaceViewModel(name, _explorer.Settings, _displayPreferences);
        renderingSurfaceViewModel.Name = name;
        var sharedExplorer = _explorer.ShareWithNewSurface(renderingSurfaceViewModel);
        var copilotChatViewModel = new CopilotChatViewModel();
        var scopedContext = _queryEditorScopedContextFactory.Create(sharedExplorer);
        var queryEditorViewModel = new QueryEditorViewModel(sharedExplorer,
            ConsoleViewModel,
            _displayPreferences,
            content,
            scopedContext);

        var queryModel = new QueryViewModel(queryEditorViewModel,
            renderingSurfaceViewModel,
            copilotChatViewModel);

        Queries.Insert(desiredIndex, new QueryItemViewModel(name, queryModel));
        ActiveQueryIndex = desiredIndex;
        scopedContext.Messenger.Send<InitMessage>();
    }

    private QueryItemViewModel GetSelectedQuery() => Queries.ElementAt(ActiveQueryIndex);

    [RelayCommand]
    private void AddQueryHere(QueryItemViewModel model)
    {
        var indexOfThis = Queries.IndexOf(model);
        AddQuery(NewQueryName, string.Empty, indexOfThis + 1);
    }

    [RelayCommand]
    private void DeleteQuery(QueryItemViewModel model)
    {
        if (Queries.Count <= 1)
            //can't delete the last query
            return;

        var prevActiveIndex = ActiveQueryIndex;
        var indexOfThis = Queries.IndexOf(model);
        Queries.RemoveAt(indexOfThis);
        if (prevActiveIndex > indexOfThis)
            ActiveQueryIndex = prevActiveIndex - 1;
        else if (prevActiveIndex == indexOfThis) ActiveQueryIndex = Math.Clamp(prevActiveIndex, 0, Queries.Count - 1);
    }


    [RelayCommand]
    private void ChangeTab()
    {
        var choices = "Left,Top,Right,Bottom".Split(',');
        var currentIndex = Array.IndexOf(choices, TabStripPlacement);
        var placementIndex = (currentIndex + 1) % choices.Length;
        var placement = choices[placementIndex];
        TabStripPlacement = placement;
    }

    [RelayCommand]
    private void ChangeTabPlacement(string placement) => TabStripPlacement = placement;

    [RelayCommand]
    private async Task RenameQuery(QueryItemViewModel model)
    {
        var text = new RenamableText(model.Header);
        await _dialogService.ShowRenameDialogs(text);
        model.Header = text.NewText;
    }

    [RelayCommand]
    private async Task Initialize()
    {
        await _registryOperations.AssociateFileType(true);
        _preferencesManager.RetrieveUiPreferencesFromDisk();
        AddQuery(NewQueryName, string.Empty);


        _preferencesManager.EnsureDefaultFolderExists();
        ApplyUiPreferences(false);
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
        _preferencesManager.SaveUiPrefs();
    }

    [RelayCommand]
    private void ToggleWordWrap()
    {
        _preferencesManager.UIPreferences.WordWrap = !_preferencesManager.UIPreferences.WordWrap;
        ApplyUiPreferences(true);
        _preferencesManager.SaveUiPrefs();
    }

    [RelayCommand]
    private void ToggleLineNumbers()
    {
        _preferencesManager.UIPreferences.ShowLineNumbers = !_preferencesManager.UIPreferences.ShowLineNumbers;
        ApplyUiPreferences(true);
        _preferencesManager.SaveUiPrefs();
    }

    [RelayCommand]
    private async Task RegisterFileAssociation() => await _registryOperations.AssociateFileType(true);

    [RelayCommand]
    private async Task NavigateToWiki(string path) => await _dialogService.ShowHelp(path);

    [RelayCommand]
    private async Task NavigateToUri(string path) => await _launcher.LaunchUriAsync(new Uri(path));

    private void ApplyUiPreferences(bool skipGrid)
    {
        var uiPreferences = _preferencesManager.UIPreferences;
        _displayPreferences.FontFamily = uiPreferences.FontFamily;
        _displayPreferences.FontSize = uiPreferences.FontSize;
        _displayPreferences.WordWrap = uiPreferences.WordWrap;
        _displayPreferences.ShowLineNumbers = uiPreferences.ShowLineNumbers;
        if (skipGrid)
            return;
    }

    private InteractiveTableExplorer CreateExplorer() =>
        new(
            ConsoleViewModel,
            _workspaceManager.Settings,
            _commandProcessor,
            new NullResultRenderingSurface());

    private async Task LoadWorkspace(string path)
    {
        if (await OfferSaveOfCurrentWorkspace() == YesNoCancel.Cancel)
            return;

        _explorer = CreateExplorer();
        //make sure we have the most recent global preferences
        var appPrefs = _preferencesManager.FetchApplicationPreferencesFromDisk();
        _workspaceManager.Load(path);
        CurrentWorkspace = _workspaceManager.Workspace;

        //reset the list of queries
        Queries = [];

        // AsyncRelayCommand<T> has an IsRunning property
        await _explorer.RunInput(appPrefs.StartupScript);
        await _explorer.RunInput(_workspaceManager.Workspace.StartupScript);

        if (CurrentWorkspace.Queries.Any())
            foreach (var p in CurrentWorkspace.Queries)
                AddQuery(p.Name, p.Text);
        else
            AddQuery("query", CurrentWorkspace.Text);
        ActiveQueryIndex = 0;

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
    }

    private bool RecheckDirty()
    {
        IsDirty = Queries.Any(q => q.QueryModel.IsDirty());
        return IsDirty;
    }

    /// <summary>
    ///     Allow the user to save any pending changes
    /// </summary>
    /// <returns>
    ///     true if the user did the save or didn't need to
    /// </returns>
    private async Task<YesNoCancel> OfferSaveOfCurrentWorkspace()
    {
        if (!RecheckDirty())
            return YesNoCancel.Yes;

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
        if (!RecheckDirty())
            return YesNoCancel.Yes;

        if (_workspaceManager.IsNewWorkspace)
            return await SaveAs();

        SaveWorkspace(_workspaceManager.Path);
        return YesNoCancel.Yes;
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

        if (result?.TryGetLocalPath() is string path)
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
        var queries = Queries
            .Select(q => new PersistedQuery(q.Header, q.QueryModel.GetText()))
            .ToArray();
        CurrentWorkspace.Queries = queries;
        _workspaceManager.Save(path, CurrentWorkspace);
        foreach (var queryItemViewModel in Queries) queryItemViewModel.QueryModel.Clean();
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


    [RelayCommand]
    private async Task FlyoutCurrentResult()
    {
        var model = GetSelectedQuery();

        var result = model.QueryModel.RenderingSurfaceViewModel.Result;
        await _dialogService.FlyoutResult(model.Header, result, _explorer.Settings, _displayPreferences);
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
    private void ChangeLayout() => WeakReferenceMessenger.Default.Send(new LayoutChangedMessage(0));

    [RelayCommand]
    private void ClearConsole() => WeakReferenceMessenger.Default.Send(new ClearConsoleMessage(0));
}

public record PersistedQuery(string Name, string Text);

public class RenamableText
{
    public string InitialText = string.Empty;
    public string NewText;

    public RenamableText(string initialText)
    {
        InitialText = initialText;
        NewText = initialText;
    }
}

// Create a message
public class LayoutChangedMessage(int layout) : ValueChangedMessage<int>(layout);

public class TabChangedMessage(int layout) : ValueChangedMessage<int>(layout);

public class ClearConsoleMessage(int layout) : ValueChangedMessage<int>(layout);

public class RunningQueryMessage(bool isRunning) : AsyncRequestMessage<bool>
{
    public bool IsRunning { get; } = isRunning;
}
