using System.Runtime.InteropServices;
using Xunit;

namespace IntellisenseTests.Platforms;

public sealed class WindowsOnlyFactAttribute : FactAttribute
{
    public WindowsOnlyFactAttribute()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        Skip = PlatformHelper.WindowsSkipMessage;
    }
}