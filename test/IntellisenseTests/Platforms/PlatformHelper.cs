using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace IntellisenseTests.Platforms;

public static class PlatformHelper
{
    public const string WindowsSkipMessage = "Windows tests are not run on non-Windows platforms";
    public const string CiSkipMessage = "Cannot run these outside of CI. Set environment variable CI='true' to run.";

    public static bool IsCi() => Environment.GetEnvironmentVariable("CI") is "true";

    public static bool IsWindowsAdmin()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
