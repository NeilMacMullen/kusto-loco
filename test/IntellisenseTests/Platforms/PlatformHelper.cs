using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace IntellisenseTests.Platforms;

public static class PlatformHelper
{
    public const string WindowsSkipMessage = "Windows tests are not run on non-Windows platforms";
    public const string AdminSkipMessage = "Cannot run tests as non-admin.";

    public static bool IsCi() => Environment.GetEnvironmentVariable("CI") is {} s && bool.TryParse(s, out var isCi) && isCi;

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
