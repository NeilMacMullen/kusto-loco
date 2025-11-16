using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using LokqlDx.Models;
using LokqlDx.ViewModels;
using LokqlDx.ViewModels.Dialogs;
using LokqlDx.Views.Dialogs;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace LokqlDx.Services;

public class DialogService
{
    private readonly ILauncher _launcher;

    private readonly ShowOptions _modalNonResizable = new(false, new Size(0, 0), true);
    private readonly ShowOptions _nonModalResizable = new(false, new Size(0, 0), true);
    private readonly TopLevel _topLevel;

    public DialogService(TopLevel topLevel, ILauncher launcher)
    {
        _topLevel = topLevel;
        _launcher = launcher;
    }

    public static FilePickerFileType All { get; } = new("All data types")
    {
        Patterns = ["*.csv", "*.tsv", "*.parquet", "*.xlsx", "*.json", "*.txt"]
    };

    public static FilePickerFileType CsvFiles { get; } = new("Csv (*.csv)")
    {
        Patterns = ["*.csv"]
    };

    public static FilePickerFileType TsvFiles { get; } = new("Tsv (*.tsv)")
    {
        Patterns = ["*.tsv"]
    };

    public static FilePickerFileType ParquetFiles { get; } = new("Parquet (*.parquet)")
    {
        Patterns = ["*.parquet"]
    };

    public static FilePickerFileType ExcelFiles { get; } = new("Excel (*.xlsx)")
    {
        Patterns = ["*.xlsx"]
    };

    public static FilePickerFileType JsonFiles { get; } = new("Json array (*.json)")
    {
        Patterns = ["*.json"]
    };
    public static FilePickerFileType JsonLFiles { get; } = new("JsonL (*.jsonl)")
    {
        Patterns = ["*.jsonl"]
    };

    public static FilePickerFileType TextFiles { get; } = new("Text (*.txt)")
    {
        Patterns = ["*.txt"]
    };

    public static FilePickerFileType[] DataTypesForRead { get; } =
    [
        All,
        TsvFiles, CsvFiles, ParquetFiles, ExcelFiles, JsonFiles,JsonLFiles, TextFiles
    ];

    public static FilePickerFileType[] DataTypesForWrite { get; } =
        DataTypesForRead.Append(All).ToArray();

    public async Task<IReadOnlyList<IStorageFile>> OpenDataFiles()
    {
        // Start async operation to open the dialog.
        var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = true,
            FileTypeFilter = DataTypesForRead
        });
        return files;
    }

    public async Task<IStorageFile?> SaveDataFiles()
    {
        // Start async operation to open the dialog.
        var files = await _topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Save File",
                FileTypeChoices = DataTypesForWrite,
                ShowOverwritePrompt = true
            });
        return files;
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
                new ShowOptions(true, new Size(600, 400), false))
            .ConfigureAwait(false);


    public async Task ShowAppPreferences(PreferencesManager preferencesManager) =>
        await ShowDialog(
                "LokqlDX - Application Options",
                new ApplicationPreferencesView(),
                new ApplicationPreferencesViewModel(preferencesManager),
                _modalNonResizable
            )
            .ConfigureAwait(false);


    public async Task ShowRenameDialogs(RenamableText initialText) =>
        await ShowDialog(
                "Rename",
                new RenameDialog(),
                new RenameDialogModel(initialText),
                _modalNonResizable)
            .ConfigureAwait(false);

    public async Task
        ShowWorkspacePreferences(LokqlDx.WorkspaceManager workspaceManager, UIPreferences uiPreferences) =>
        await ShowDialog(
                "LokqlDX - Workspace Options",
                new WorkspacePreferencesView(),
                new WorkspacePreferencesViewModel(workspaceManager, uiPreferences),
                _modalNonResizable)
            .ConfigureAwait(false);

    private async Task ShowDialog(string title, Control content, IDialogViewModel dataContext, ShowOptions options)
    {
        if (_topLevel is Window window)
        {
            var sizing = options.InitialSize.Height != 0
                ? SizeToContent.Manual
                : SizeToContent.WidthAndHeight;

            var dialog = new Window
            {
                Title = title,
                Content = content,
                DataContext = dataContext,
                SizeToContent = sizing,
                CanResize = options.CanResize,
                TransparencyLevelHint =
                    [WindowTransparencyLevel.Mica, WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur],
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (options.InitialSize.Width > 0)
            {
                dialog.Width = options.InitialSize.Width;
                dialog.Height = options.InitialSize.Height;
            }

            if (window.ActualTransparencyLevel != WindowTransparencyLevel.Mica)
                dialog[!TemplatedControl.BackgroundProperty]
                    = new DynamicResourceExtension("SystemControlBackgroundAltMediumHighBrush");
            else
                dialog.Background = Brushes.Transparent;
#if DEBUG
#if !NCRUNCH
            dialog.AttachDevTools();
#endif
#endif

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            if (!options.Modal)
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

    private readonly record struct ShowOptions(bool CanResize, Size InitialSize, bool Modal);
}
