using Microsoft.Win32;
using System.Security.Principal;
using System.Windows;

namespace lokqlDx;

public static class RegistryOperations
{

    public static bool IsAdmin()
    {
        // Check if we are running as administrator
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void AssociateFileType()
    {
        //taken from https://stackoverflow.com/questions/1387769/create-registry-entry-to-associate-file-extension-with-application-in-c

        if (!IsAdmin())
        {
            MessageBox.Show("The application needs to be run in administrator mode to register for file associations");
            return;
        }
        const string progId = "kustoloco.lokqldx";
        var exe = System.Diagnostics.Process.GetCurrentProcess()?.MainModule?.FileName ?? string.Empty;

        Registry.SetValue($@"HKEY_CURRENT_USER\Software\Classes\{progId}\shell\open\command",
            null,
            "\""+exe + "\" \"%1\"");

        Registry.SetValue($@"HKEY_CURRENT_USER\Software\Classes\.{WorkspaceManager.Extension}", null, progId);
        MessageBox.Show($"Lokqldx now registered with .{WorkspaceManager.Extension} files ");
    }

}
