using System;
using FluentAssertions;
using Intellisense.FileSystem.Paths;
using IntellisenseTests.Platforms;
using Xunit;

namespace IntellisenseTests;

public class UncPathTests
{

    [WindowsOnlyTheory]
    [InlineData(@"\\127", "127")]
    [InlineData(@"\\127.\", "127.")]
    [InlineData(@"\\127.", "127.")]
    [InlineData(@"\\127.0.0.1\Share", "127.0.0.1")]
    [InlineData(@"\\127.\Share", "127.")]
    [InlineData(@"\\127\Share", "127")]
    public void Host_ReturnsHostSegmentWithoutCoercingPartialAddressesIntoFullAddresses(
        string uriString,
        string expected
    )
    {
        var uri = new Uri(uriString);

        var unc = new UncPath(uri);

        unc.OriginalHost.Should().Be(expected);
    }
}
