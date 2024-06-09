﻿using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using KustoLoco.Core;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering;
using Lokql.Engine;
using Lokql.Engine.Commands;
using Microsoft.Win32;

namespace lokqlDx;

public partial class MainWindow : Window
{
    private readonly string[] _args;
    private readonly WpfConsole _console;
    private readonly PreferencesManager _preferenceManager = new();
    private readonly WorkspaceManager _workspaceManager;
    private InteractiveTableExplorer _explorer;

    private bool isBusy;


    public MainWindow(
        string[] args
    )
    {
        _args = args.ToArray();
        InitializeComponent();
        _console = new WpfConsole(OutputText);
        var settings = new KustoSettingsProvider();
        _workspaceManager = new WorkspaceManager(settings);
        var loader = new StandardFormatAdaptor(settings, _console);
        var cp = CommandProcessor.Default();
        _explorer = new InteractiveTableExplorer(_console, loader, settings, cp);
    }

    private async Task RunQuery(string query)
    {
        if (isBusy)
            return;
        isBusy = true;
        Editor.SetBusy(true);
        //start capturing console output from the engine
        _console.PrepareForOutput();
        //run the supplied lines of kusto/commands
        //Note that we need the extra Task.Run here to ensure
        //that the UI thread is not blocked for reports generated by
        //the engine

        await Task.Run(async () => await _explorer.RunInput(query));
        var result = _explorer._prevResult;
        //if there are no results leave the previously rendered results in place
        if (result.RowCount != 0)
        {
            if (result.Visualization != VisualizationState.Empty)
            {
                //annoying we have to do this, but it's the only way to get the webview to render
                await webview.EnsureCoreWebView2Async();
                //generate the html and display it
                var renderer = new KustoResultRenderer(_explorer.Settings);
                var html = renderer.RenderToHtml(result);
                try
                {
                    webview.NavigateToString(html);
                }
                catch
                {
                    _explorer.Warn("Unable to render results in webview");
                }
            }

            FillInDataGrid(result);
        }

        Editor.SetBusy(false);
        isBusy = false;
    }

    private void FillInDataGrid(KustoQueryResult result)
    {
        var maxDataGridRows = int.TryParse(VisibleDataGridRows.Text, out var parsed) ? parsed : 100;
        var dt = result.ToDataTable(maxDataGridRows);
        dataGrid.ItemsSource = dt.DefaultView;
    }

    /// <summary>
    ///     Called when user presses CTRL-ENTER in the query editor
    /// </summary>
    private async void OnQueryEditorRunTextBlock(object? sender, QueryEditorRunEventArgs eventArgs)
    {
        await RunQuery(eventArgs.Query);
    }

    private void UpdateUIFromWorkspace()
    {
        Editor.SetText(_workspaceManager.UserText);
        var settings = _workspaceManager.Settings;
        var loader = new StandardFormatAdaptor(settings, _console);
        _explorer = new InteractiveTableExplorer(_console, loader, settings, CommandProcessor.Default());
        UpdateFontSize();
        Title = $"LokqlDX - {_workspaceManager.Path}";
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        _preferenceManager.Load();
        var pathToLoad = _args.Any() ? _args[0] : _preferenceManager.Preferences.LastWorkspacePath;
        _workspaceManager.Load(pathToLoad);
        if (Width > 100 && Height > 100 && Left > 0 && Top > 0)
        {
            Width = _preferenceManager.Preferences.WindowWidth;
            Height = _preferenceManager.Preferences.WindowHeight;
            Left = _preferenceManager.Preferences.WindowLeft;
            Top = _preferenceManager.Preferences.WindowTop;
        }

        UpdateUIFromWorkspace();
        await Navigate("https://github.com/NeilMacMullen/kusto-loco/wiki/LokqlDX");
    }

    private void SaveWorkspace(string path)
    {
        PreferencesManager.EnsureDefaultFolderExists();
        _workspaceManager.Save(path, Editor.GetText());
        _preferenceManager.Preferences.LastWorkspacePath = _workspaceManager.Path;
        _preferenceManager.Preferences.WindowLeft = Left;
        _preferenceManager.Preferences.WindowTop = Top;
        _preferenceManager.Preferences.WindowWidth = Width;
        _preferenceManager.Preferences.WindowHeight = Height;

        _preferenceManager.Save();
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        Save();
    }

    private void OpenWorkSpace(object sender, RoutedEventArgs e)
    {
        var folder = _workspaceManager.ContainingFolder();
        var dialog = new OpenFileDialog
        {
            InitialDirectory = folder,
            Filter = $"Lokql Workspace ({WorkspaceManager.GlobPattern})|{WorkspaceManager.GlobPattern}",
            FileName = Path.GetFileName(_workspaceManager.Path)
        };

        if (dialog.ShowDialog() == true)
        {
            _workspaceManager.Load(dialog.FileName);
            UpdateUIFromWorkspace();
        }
    }

    private void EditPreferences(object sender, RoutedEventArgs e)
    {
    }

    private void SaveWorkspace(object sender, RoutedEventArgs e)
    {
        Save();
    }

    private void Save()
    {
        JumpList.AddToRecentCategory(_workspaceManager.Path);
        SaveWorkspace(_workspaceManager.Path);
    }

    private bool SaveAs()
    {
        var folder = _workspaceManager.ContainingFolder();
        var dialog = new SaveFileDialog
        {
            InitialDirectory = folder,
            Filter = $"Lokql Workspace ({WorkspaceManager.GlobPattern})|{WorkspaceManager.GlobPattern}",
            FileName = Path.GetFileName(_workspaceManager.Path)
        };
        if (dialog.ShowDialog() == true)
        {
            SaveWorkspace(dialog.FileName);
            return true;
        }

        return false;
    }

    private void SaveWorkspaceAs(object sender, RoutedEventArgs e)
    {
        SaveAs();
    }

    private void NewWorkspace(object sender, RoutedEventArgs e)
    {
        //save current
        Save();
        var prevPath = _workspaceManager.Path;
        _workspaceManager.CreateNewInCurrentFolder();
        if (SaveAs())
        {
            UpdateUIFromWorkspace();
            _explorer.SetWorkingPaths(_workspaceManager.ContainingFolder());
        }
        else
        {
            _workspaceManager.Load(prevPath);
            UpdateUIFromWorkspace();
        }
    }

    private void IncreaseFontSize(object sender, RoutedEventArgs e)
    {
        _preferenceManager.Preferences.FontSize = Math.Min(40, _preferenceManager.Preferences.FontSize + 1);
        UpdateFontSize();
    }

    private void DecreaseFontSize(object sender, RoutedEventArgs e)
    {
        _preferenceManager.Preferences.FontSize = Math.Max(6, _preferenceManager.Preferences.FontSize - 1);
        UpdateFontSize();
    }

    private void UpdateFontSize()
    {
        Editor.SetFontSize(_preferenceManager.Preferences.FontSize);
        OutputText.FontSize = _preferenceManager.Preferences.FontSize;
        dataGrid.FontSize = _preferenceManager.Preferences.FontSize;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // here I suppose the window's menu is named "MainMenu"
        MainMenu.RaiseMenuItemClickOnKeyGesture(e);
    }

    private async Task Navigate(string url)
    {
        await webview.EnsureCoreWebView2Async();
        webview.Source = new Uri(url);
    }

    private async void NavigateToGettingStarted(object sender, RoutedEventArgs e)
    {
        await Navigate("https://github.com/NeilMacMullen/kusto-loco/wiki/LokqlDX");
    }

    private async void NavigateToProjectPage(object sender, RoutedEventArgs e)
    {
        await Navigate("https://github.com/NeilMacMullen/kusto-loco");
    }

    private async void NavigateToKqlIntroductionPage(object sender, RoutedEventArgs e)
    {
        await Navigate(
            "https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/tutorials/learn-common-operators");
    }

    private void EnableJumpList(object sender, RoutedEventArgs e)
    {
        RegistryOperations.AssociateFileType();
    }
}

public static class MenuExtensions
{
    public static void RaiseMenuItemClickOnKeyGesture(this ItemsControl? control, KeyEventArgs args)
    {
        RaiseMenuItemClickOnKeyGesture(control, args, false);
    }

    public static void RaiseMenuItemClickOnKeyGesture(this ItemsControl? control, KeyEventArgs args, bool throwOnError)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (control == null)
            return;

        var kgc = new KeyGestureConverter();
        foreach (var item in control.Items.OfType<MenuItem>())
        {
            if (!string.IsNullOrWhiteSpace(item.InputGestureText))
            {
                KeyGesture? gesture = null;
                if (throwOnError)
                    gesture = kgc.ConvertFrom(item.InputGestureText) as KeyGesture;
                else
                    try
                    {
                        gesture = kgc.ConvertFrom(item.InputGestureText) as KeyGesture;
                    }
                    catch
                    {
                    }


                if (gesture != null && gesture.Matches(null, args)
                   )
                {
                    item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    args.Handled = true;
                    return;
                }
            }

            RaiseMenuItemClickOnKeyGesture(item, args, throwOnError);
            if (args.Handled)
                return;
        }
    }
}
