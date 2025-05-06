using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace IntellisenseTests.Platforms;

public sealed class WindowsOnlyTheoryAttribute : TheoryAttribute
{
    public WindowsOnlyTheoryAttribute()
    {
        if (MockUnixSupport.IsWindowsPlatform())
        {
            return;
        }

        Skip = TestConstants.WindowsSkipMessage;
    }
}
