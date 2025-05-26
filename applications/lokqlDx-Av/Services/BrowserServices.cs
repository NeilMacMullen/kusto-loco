using System.Diagnostics;

namespace LokqlDx.Services;

public class BrowserServices {

    public BrowserServices()
    {
    }

    public void OpenUriInBrowser(string uri) =>
        Process.Start(new ProcessStartInfo { FileName = uri, UseShellExecute = true });
}

