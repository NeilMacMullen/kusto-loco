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

        if (!IsAdmin())
        {
            MessageBox.Show("The application needs to be run in administrator mode to register for file associations");
            return;
        }
        const string progId = "kustoloco.lokqldx";

        // Check if we have write access to the registry
        var key = Registry.CurrentUser.OpenSubKey("Software", true);
        if (key == null)
        {
            // We don't have access
            return;
        }

        // Create a value for this key that points to the ProgId
        key = key.CreateSubKey($".{WorkspaceManager.Extension}");
        if (key != null) key.SetValue("", progId);

        // Create a new key for the ProgId
        key = Registry.ClassesRoot.CreateSubKey(progId);
        if (key == null) return;

        // Set the default value of this key to the application path
        key.SetValue("", System.Diagnostics.Process.GetCurrentProcess()?.MainModule?.FileName ??string.Empty);
        MessageBox.Show($"Lokqldx now registered with .{WorkspaceManager.Extension} files ");
    }

}
