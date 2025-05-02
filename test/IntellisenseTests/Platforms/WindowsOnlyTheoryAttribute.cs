using System.Runtime.InteropServices;
using Xunit;

namespace IntellisenseTests.Platforms;

public sealed class WindowsOnlyTheoryAttribute : TheoryAttribute
{
    public WindowsOnlyTheoryAttribute()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        Skip = PlatformHelper.WindowsSkipMessage;
    }
}
