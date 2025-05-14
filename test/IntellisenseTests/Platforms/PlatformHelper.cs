using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace IntellisenseTests.Platforms;

public static class PlatformHelper
{
    public const string WindowsSkipMessage = "Windows tests are not run on non-Windows platforms";
    public const string AdminSkipMessage = "Cannot run tests as non-admin.";
    public const string CiSkipMessage = "Cannot run these outside of CI. Set environment variable CI='true' to run.";

    public static bool IsCi() => IsTruthy(Environment.GetEnvironmentVariable("CI"));

    public static bool IsWindowsAdmin()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static bool IsTruthy(string? value)
    {
        if (value is null)
        {
            return false;
        }

        if (value is "1")
        {
            return true;
        }

        if (value is "0")
        {
            return false;
        }

        if (!bool.TryParse(value, out var isTruthy))
        {
            return false;
        }

        return isTruthy;
    }
}
