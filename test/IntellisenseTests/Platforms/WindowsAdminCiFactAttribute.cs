using System;
using System.Runtime.InteropServices;
using Xunit;

namespace IntellisenseTests.Platforms;

public sealed class WindowsAdminCiFactAttribute : FactAttribute
{
    public WindowsAdminCiFactAttribute()
    {
        if (!PlatformHelper.IsCi())
        {
            Skip = PlatformHelper.CiSkipMessage;
            return;
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = PlatformHelper.WindowsSkipMessage;
            return;
        }

        if (!PlatformHelper.IsWindowsAdmin())
        {
            throw new InvalidOperationException("Windows CI should not skip admin tests");
        }
    }
}
