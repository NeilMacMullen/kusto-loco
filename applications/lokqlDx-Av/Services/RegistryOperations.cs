using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;

namespace LokqlDx.Services;

public class RegistryOperations
{
    private readonly DialogService _dialogService;

    public RegistryOperations(DialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public bool IsAdmin()
    {
        if (OperatingSystem.IsWindows())
        {
            // Check if we are running as administrator
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        return false;
    }

    public async Task AssociateFileType(bool quiet)
    {
        if (!OperatingSystem.IsWindows())
            return;

        //taken from https://stackoverflow.com/questions/1387769/create-registry-entry-to-associate-file-extension-with-application-in-c
        try
        {
            const string progId = "kustoloco.lokqldx";
            var exe = Process.GetCurrentProcess()?.MainModule?.FileName ?? string.Empty;

            Registry.SetValue($@"HKEY_CURRENT_USER\Software\Classes\{progId}\shell\open\command",
                null,
                "\"" + exe + "\" \"%1\"");

            Registry.SetValue($@"HKEY_CURRENT_USER\Software\Classes\.{WorkspaceManager.Extension}", null, progId);
            if (!quiet)
                await _dialogService.ShowMessageBox(
                    $"Lokqldx now registered with .{WorkspaceManager.Extension} files ");
        }
        catch
        {
            if (!quiet)
                await _dialogService.ShowMessageBox(
                    "Unable to register file association - you may need to run the application as admin");
        }
    }
}
