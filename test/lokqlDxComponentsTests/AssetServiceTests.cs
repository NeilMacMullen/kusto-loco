using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using Jab;
using lokqlDxComponents.Services;
using lokqlDxComponentsTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace lokqlDxComponentsTests;

public class AssetServiceTests
{
    [AvaloniaFact]
    public void Open_CurrentAssemblyMissingAsset_SearchesUpstreamAssemblies()
    {
        var f = new AssetServiceTestContainer();
        var locator = f.GetRequiredService<IAssetService>();
        locator
            .Invoking(x => x.Open("my_unique_file_151.txt"))
            .Should()
            .NotThrow()
            .Which.Should()
            .NotHaveLength(0);
    }

    [AvaloniaFact]
    public void Open_ResourceFoundInCurrentAndUpstreamAssembly_PrioritizesDownstreamAssembly()
    {
        var f = new AssetServiceTestContainer();
        var locator = f.GetRequiredService<IAssetService>();
        var res = locator.Open("dummy_file.txt");
        new StreamReader(res).ReadToEnd().Should().Be("MyTestVal1");
    }

    [AvaloniaFact]
    public void Open_ResourceNotFound_Throws()
    {
        var f = new AssetServiceTestContainer();
        var locator = f.GetRequiredService<IAssetService>();
        locator
            .Invoking(x => x.Open("NonexistentFile.txt"))
            .Should()
            .Throw<FileNotFoundException>("Most usages should be with assets that are statically known to exist.")
            .Which.FileName.Should()
            .Be("NonexistentFile.txt");
    }

    [AvaloniaFact]
    public void GetAssetsByFolder_ReturnsAllAssetsInFolder()
    {
        var f = new AssetServiceTestContainer();
        var locator = f.GetRequiredService<IAssetService>();

        locator
            .GetAssetPathsByFolder("AFolder1")
            .ShouldBeEquivalentToPaths("/AFolder1/dummy_file2.txt", "/AFolder1/dummy_file3.txt");
    }
}

[ServiceProvider]
[Import<IBaseTestModule>]
[Singleton<IInternalAssetLoader>(Factory = nameof(GetAssetLoader))]
[Singleton<InternalAssetLoader>]
public partial class AssetServiceTestContainer
{
    public Func<IServiceProvider, IInternalAssetLoader> GetAssetLoader = sp =>
        sp.GetRequiredService<InternalAssetLoader>();
}
