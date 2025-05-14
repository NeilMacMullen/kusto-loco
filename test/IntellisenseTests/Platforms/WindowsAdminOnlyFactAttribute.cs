using System;
using System.Runtime.InteropServices;
using Xunit;

namespace IntellisenseTests.Platforms;

public sealed class WindowsAdminOnlyFactAttribute : FactAttribute
{
    public WindowsAdminOnlyFactAttribute()
    {
        if (!PlatformHelper.IsCi())
        {
            Skip = PlatformHelper.CiSkipMessage;
            return;
        }

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        if (!isWindows)
        {
            Skip = PlatformHelper.WindowsSkipMessage;
            return;
        }

        var isAdmin = PlatformHelper.IsWindowsAdmin();

        if (!isAdmin)
        {
            throw new InvalidOperationException("Windows CI should not skip admin tests");
        }
    }
}
