using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace LokqlDx.ViewModels.Dialogs;

public partial class MarkDownHelpModel : ObservableObject, IDialogViewModel
{
    private readonly TaskCompletionSource _completionSource;

    private const string BrowserPrefix = @"https://github.com/NeilMacMullen/kusto-loco/wiki";
    private const string RawPrefix = @"https://raw.githubusercontent.com/wiki/NeilMacMullen/kusto-loco";
  
    private readonly string _link;

    public MarkDownHelpModel(string link)
    {
        _link = link;
        _completionSource = new TaskCompletionSource();
        Result = _completionSource.Task;
        _markdownText = string.Empty;
    }


    public Task Result { get; }

    [ObservableProperty] public string _markdownText;
    
    [RelayCommand]
    private void Save()
    {
    }

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
            MarkdownText=text;
        }
        catch
        {
            MarkdownText = "Error loading page";
        }
    }
    [RelayCommand]
    public void ShowInBrowser()
    {
        var link = MakeUri(BrowserPrefix);
        OpenUriInBrowser(link);
    }

    private static void OpenUriInBrowser(string uri) =>
        Process.Start(new ProcessStartInfo { FileName = uri, UseShellExecute = true });
}
