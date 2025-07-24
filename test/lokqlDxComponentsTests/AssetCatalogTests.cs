using AwesomeAssertions;
using AwesomeAssertions.Execution;
using lokqlDxComponents.Models;
using lokqlDxComponents.Services.Assets;

namespace lokqlDxComponentsTests;

public class AssetCatalogTests
{

    [Fact]
    public void Catalog_TakesPathsRelativeToSecondUriSegment()
    {
        // 2nd uri segment == Assets
        var uris = new[]
        {
            "avares://myAssembly/Assets/file.txt",
            "avares://myAssembly/Assets/folder/file.txt",
            "avares://myAssembly/Assets/folder/file2.txt"
        };
        var catalog = AssetCatalog.Create(uris);


        var input = "folder/file.txt";
        var input2 = "folder";
        using var _ = new AssertionScope();
        catalog.GetAsset(input).ResourcePath.ShouldBeEquivalentToPath(input);
        catalog.Contains(input).Should().BeTrue();
        catalog
            .GetAssetsByFolder(input2)
            .Select(x => x.ResourcePath)
            .ShouldBeEquivalentToPaths("folder/file.txt", "folder/file2.txt");
    }

    [Fact]
    public void Catalog_IsCaseInsensitive()
    {
        var uris = new[]
        {
            "avares://myAssembly/Assets/file.txt",
            "avares://myAssembly/Assets/folder/file.txt",
            "avares://myAssembly/Assets/folder/file2.txt"
        };
        var catalog = AssetCatalog.Create(uris);


        var input = "folder/file.txt".ToUpper();
        var input2 = "folder".ToUpper();
        using var _ = new AssertionScope();
        catalog.GetAsset(input).ResourcePath.ShouldBeEquivalentToPath("folder/file.txt");
        catalog.Contains(input).Should().BeTrue();
        catalog
            .GetAssetsByFolder(input2)
            .Select(x => x.ResourcePath)
            .ShouldBeEquivalentToPaths("folder/file.txt", "folder/file2.txt");
    }


    [Theory]
    [InlineData("AFolder1\\")]
    [InlineData("AFolder1")]
    [InlineData("/AFolder1\\")]
    public void Catalog_IgnoresSeparatorsAtEnds(string input)
    {
        var uris = new[]
        {
            "avares://myAssembly/Assets/AFolder1/dummy1.txt",
            "avares://myAssembly/Assets/AFolder1/dummy2.txt",
            "avares://myAssembly/Assets/MyFolder2/dummy4.txt"
        };
        var catalog = AssetCatalog.Create(uris);

        var assets = catalog.GetAssetsByFolder(input);

        assets.Should().HaveCount(2);
    }

    [Fact]
    public void Catalog_ThrowsOnDuplicatesCaseInsensitive()
    {
        var uris = new[]
        {
            "avares://myAssembly/Assets/File1.txt",
            "avares://myAssembly/Assets/FILE1.TXT"
        };

        uris.Invoking(AssetCatalog.Create).Should().Throw<ArgumentException>();
    }
}
