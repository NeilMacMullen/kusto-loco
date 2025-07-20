using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using Jab;
using lokqlDxComponents;
using lokqlDxComponents.Services;
using lokqlDxComponentsTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace lokqlDxComponentsTests;

public class AssetServiceTests
{
    [AvaloniaFact]
    public void Open_SearchesUpstreamAssemblies()
    {
        var f = new AssetServiceTestContainer();
        var locator = f.GetRequiredService<IAssetService>();
        locator.Open(AssetLocations.SyntaxHighlighting).Should().NotHaveLength(0);
    }

    [AvaloniaFact]
    public void Open_ResourceFoundInMultipleAssemblies_PrioritizesDownstreamAssembly()
    {
        var f = new AssetServiceTestContainer();
        var locator = f.GetRequiredService<IAssetService>();
        var res = locator.Open("/Assets/dummy_file.txt");
        new StreamReader(res).ReadToEnd().Should().Be("MyTestVal1");
    }

    [AvaloniaFact]
    public void Exists_InvalidPath_ReturnsFalse()
    {
        var f = new AssetServiceTestContainer();
        var mock = new Mock<IInternalAssetLoader>();
        mock.Setup(x => x.Exists(It.IsAny<Uri>())).Throws<Exception>();
        var locator = f.GetRequiredService<IAssetService>();

        locator.Exists("ad/df/weaf/ghew/qdas").Should().BeFalse();
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