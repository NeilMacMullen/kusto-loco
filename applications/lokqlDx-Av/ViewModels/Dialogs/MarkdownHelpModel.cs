using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LokqlDx.ViewModels.Dialogs;

public partial class MarkDownHelpModel : ObservableObject, IDialogViewModel
{
    private const string BrowserPrefix = @"https://github.com/NeilMacMullen/kusto-loco/wiki";
    private const string RawPrefix = @"https://raw.githubusercontent.com/wiki/NeilMacMullen/kusto-loco";
    private readonly ILauncher _launcher;

    private readonly string _link;
    [ObservableProperty] public string _markdownText;

    public MarkDownHelpModel(string link, ILauncher launcher)
    {
        _link = link;
        _launcher = launcher;
        _markdownText = string.Empty;
    }


    public Task Result { get; } = Task.CompletedTask;

    private string MakeUri(string prefix)
    {
        var escaped = Uri.EscapeDataString(_link);
        return $"{prefix}/{escaped}";
    }

    [RelayCommand]
    public async Task FetchMarkdown()
    {
        try
        {
            var rawLink = MakeUri(RawPrefix) + ".md";
            using var client = new HttpClient();
            var text = await client.GetStringAsync(rawLink);
            MarkdownText = text;
        }
        catch
        {
            MarkdownText = "Error loading page";
        }
    }

    [RelayCommand]
    public async Task ShowInBrowser()
    {
        var link = MakeUri(BrowserPrefix);
        await _launcher.LaunchUriAsync(new Uri(link));
    }
}
