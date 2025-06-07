using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using LokqlDx.Models;
using LokqlDx.ViewModels;
using LokqlDx.ViewModels.Dialogs;
using LokqlDx.Views.Dialogs;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NotNullStrings;
using Flyout = LokqlDx.Views.Flyout;

namespace LokqlDx.Services;

public class DialogService
{
    private readonly ILauncher _launcher;
    private readonly TopLevel _topLevel;

    public DialogService(TopLevel topLevel, ILauncher launcher)
    {
        _topLevel = topLevel;
        _launcher = launcher;
    }

    public Task ShowMessageBox(string message)
    {
        var mb = MessageBoxManager.GetMessageBoxStandard("lokqlDx", message);

        return mb.ShowAsync();
    }

    public Task ShowMessageBox(string title, string message)
    {
        var mb = MessageBoxManager.GetMessageBoxStandard(title, message);

        return mb.ShowAsync();
    }

    public async Task<YesNoCancel> ShowConfirmCancelBox(string title, string message)
    {
        var mb = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNoCancel);

        var result = await mb.ShowAsync();

        return result switch
        {
            ButtonResult.Ok or ButtonResult.Yes => YesNoCancel.Yes,
            ButtonResult.No => YesNoCancel.No,
            _ => YesNoCancel.Cancel
        };
    }


    public async Task ShowHelp(string page) =>
        await ShowDialog(
                page,
                new MarkdownHelpWindow(),
                new MarkDownHelpModel(page, _launcher),
                true)
            .ConfigureAwait(false);



    public async Task FlyoutResult(KustoQueryResult result, KustoSettingsProvider explorerSettings)
    {
        //use the first line of the query as the title
        var title = result.Query.Tokenize("\r\n").FirstOrDefault("result");
        await ShowDialog(
               title,
                new Flyout(),
                new FlyoutViewModel(result, explorerSettings),
                true)
            .ConfigureAwait(false);
    }

    public async Task ShowAppPreferences(PreferencesManager preferencesManager) =>
        await ShowDialog(
                "LokqlDX - Application Options",
                new ApplicationPreferencesView(),
                new ApplicationPreferencesViewModel(preferencesManager))
            .ConfigureAwait(false);

    public async Task
        ShowWorkspacePreferences(LokqlDx.WorkspaceManager workspaceManager, UIPreferences uiPreferences) =>
        await ShowDialog(
                "LokqlDX - Workspace Options",
                new WorkspacePreferencesView(),
                new WorkspacePreferencesViewModel(workspaceManager, uiPreferences))
            .ConfigureAwait(false);

    private async Task ShowDialog(string title, Control content, IDialogViewModel dataContext, bool modeless = false)
    {
        if (_topLevel is Window window)
        {
            var dialog = new Window
            {
                Title = title,
                Content = content,
                DataContext = dataContext,
                TransparencyLevelHint =
                    [WindowTransparencyLevel.Mica, WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur],
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (window.ActualTransparencyLevel != WindowTransparencyLevel.Mica)
                dialog[!TemplatedControl.BackgroundProperty]
                    = new DynamicResourceExtension("SystemControlBackgroundAltMediumHighBrush");
            else
                dialog.Background = Brushes.Transparent;

#if DEBUG
            dialog.AttachDevTools();
#endif

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            if (modeless)
            {
                dialog.Show(window);
            }
            else
            {
                await Task.WhenAny(dialog.ShowDialog(window), dataContext.Result);
                if (dialog.IsVisible)
                    dialog.Close();
            }

#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
        }
    }

}
