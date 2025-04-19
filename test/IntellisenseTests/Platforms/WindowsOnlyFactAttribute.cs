using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace IntellisenseTests.Platforms;

public sealed class WindowsOnlyFactAttribute : FactAttribute
{
    public WindowsOnlyFactAttribute()
    {
        if (MockUnixSupport.IsWindowsPlatform())
        {
            return;
        }

        Skip = TestConstants.WindowsSkipMessage;
    }
}
