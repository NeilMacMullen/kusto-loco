using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LokqlDx.Models;
using LokqlDx.ViewModels.Dialogs;
using LokqlDx.Views.Dialogs;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace LokqlDx.Services;

public class DialogService
{
    private readonly TopLevel _topLevel;

    public DialogService(TopLevel topLevel)
    {
        _topLevel = topLevel;
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
                new MarkDownHelpModel(page))
            .ConfigureAwait(false);


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

    private async Task ShowDialog(string title, Control content, IDialogViewModel dataContext)
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
                Background = window.FindResource("SystemListLowColor") as IBrush,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

#if DEBUG
            dialog.AttachDevTools();
#endif

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await Task.WhenAny(dialog.ShowDialog(window), dataContext.Result);
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

            if (dialog.IsVisible)
                dialog.Close();
        }
    }
}
