using FluentAssertions;
using Intellisense.FileSystem;
using Intellisense.FileSystem.Paths;
using Xunit;

namespace IntellisenseTests;

public class RootedPathTests
{

    [Theory]
    [InlineData("//unc/c/")]
    [InlineData("//unc/c$")] // support admin shares
    [InlineData("//unc/c$/")]
    [InlineData("//unc/c:/")]
    public void IsRootDirectory_RootPaths_IsTrue(string path)
    {
        var uncPath = RootedPath.Create(path);

        uncPath.IsRootDirectory().Should().BeTrue();
    }

    [Theory]
    [InlineData("//unc/cd/")]
    [InlineData("//unc/c$$")]
    [InlineData("//unc/c::/")]
    [InlineData("//unc/c$$/")]
    [InlineData("//unc/c:$/")]
    public void IsRootDirectory_NotRootPath_IsFalse(string path)
    {
        var uncPath = RootedPath.Create(path);

        uncPath.IsRootDirectory().Should().BeFalse();
    }
}
