using System.IO.Abstractions.TestingHelpers;
using Avalonia.Headless.XUnit;
using Avalonia.Svg;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Intellisense;
using Jab;
using LogSetup;
using lokqlDxComponents.Configuration;
using lokqlDxComponents.Services;
using lokqlDxComponents.Views;
using Microsoft.Extensions.DependencyInjection;

namespace lokqlDxComponentsTests;

public class AssetImageProviderTests
{
    // we are using svg width as a proxy for identifying the file that was loaded since SvgImage does not retain any information related to the original path.
    private const int Json = 10;
    private const int Csv = 20;
    private const int File = 30;
    private const int Folder = 40;


    private static readonly AppOptions Opts = new()
    {
        AssemblyName = nameof(lokqlDxComponentsTests)
    };

    [AvaloniaTheory]
    [InlineData("json", Json)]
    [InlineData("csv", Csv)]
    public void GetImage_File_GetsImageAssociatedWithFileExtension(string fileExtension, int expectedWidth)
    {
        var f = new ImageProviderTestContainer();
        f.Options = Opts;
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                [$"/someFolder/myFolder/aFile.{fileExtension}"] = new("")
            }
        );
        var file = fileSystem.AllFiles.Select(x => fileSystem.FileInfo.New(x)).Single();

        var service = f.GetRequiredService<AssetFolderImageProvider>();
        var fileExtensionService = f.GetRequiredService<IFileExtensionService>();

        var img = service.GetImage(fileExtensionService.GetIntellisenseHint(file));


        img.Should().BeOfType<SvgImage>().Which.Source.Picture!.CullRect.Width.Should().Be(expectedWidth);
    }

    [AvaloniaFact]
    public void GetImage_FileWithNoExtension_GetsDefaultFileImage()
    {
        var f = new ImageProviderTestContainer();
        f.Options = Opts;
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["/someFolder/myFolder/aFile"] = new("")
            }
        );
        var file = fileSystem.AllFiles.Select(x => fileSystem.FileInfo.New(x)).Single();

        var service = f.GetRequiredService<AssetFolderImageProvider>();
        var fileExtensionService = f.GetRequiredService<IFileExtensionService>();

        var img = service.GetImage(fileExtensionService.GetIntellisenseHint(file));


        img.Should().BeOfType<SvgImage>().Which.Source.Picture!.CullRect.Width.Should().Be(File);
    }

    [AvaloniaFact]
    public void GetImage_UnsupportedExtension_GetsDefaultFileImage()
    {
        var f = new ImageProviderTestContainer();
        f.Options = Opts;
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["/someFolder/aFile.ext23"] = new("")
            }
        );
        var file = fileSystem.AllFiles.Select(x => fileSystem.FileInfo.New(x)).Single();

        var service = f.GetRequiredService<AssetFolderImageProvider>();
        var fileExtensionService = f.GetRequiredService<IFileExtensionService>();

        var img = service.GetImage(fileExtensionService.GetIntellisenseHint(file));


        img.Should().BeOfType<SvgImage>().Which.Source.Picture!.CullRect.Width.Should().Be(File);
    }

    [AvaloniaFact]
    public void GetImage_Directory_GetsFolderImage()
    {
        var f = new ImageProviderTestContainer();
        f.Options = Opts;
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["/someFolder/aFile.txt"] = new("")
            }
        );
        var dir = fileSystem
            .AllDirectories.Select(x => fileSystem.DirectoryInfo.New(x))
            .Single(x => x.Name is "someFolder");

        var service = f.GetRequiredService<AssetFolderImageProvider>();
        var fileExtensionService = f.GetRequiredService<IFileExtensionService>();

        var img = service.GetImage(fileExtensionService.GetIntellisenseHint(dir));


        img.Should().BeOfType<SvgImage>().Which.Source.Picture!.CullRect.Width.Should().Be(Folder);
    }

    [AvaloniaFact(Skip = "TODO")]
    public void GetImage_SupportsRasterAndVectorFormats()
    {
        var f = new ImageProviderTestContainer();
        f.Options = Opts;
        var pngFileStr = "/someFolder/pngFile.txt";
        var svgFileStr = "/someFolder/svgFile.csv";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                [pngFileStr] = new(""),
                [svgFileStr] = new("")
            }
        );
        var pngFile = fileSystem.FileInfo.New(pngFileStr);
        var svgFile = fileSystem.FileInfo.New(svgFileStr);

        var service = f.GetRequiredService<AssetFolderImageProvider>();
        var fileExtensionService = f.GetRequiredService<IFileExtensionService>();

        var png = service.GetImage(fileExtensionService.GetIntellisenseHint(pngFile));
        var svg = service.GetImage(fileExtensionService.GetIntellisenseHint(svgFile));

        using var _ = new AssertionScope();
        svg.Should().BeOfType<SvgImage>();
        png.Should().NotBeOfType<SvgImage>().And.NotBeOfType<NullImage>();
    }

    [AvaloniaFact]
    public void GetImage_NonExistentAsset_ReturnsNullImage()
    {
        var f = new ImageProviderTestContainer();
        f.Options = new AppOptions { AssemblyName = "NonExistentAssembly" };
        var service = f.GetRequiredService<AssetFolderImageProvider>();

        var result = service.GetImage(IntellisenseHint.Csv);

        result.Should().BeOfType<NullImage>();
    }

    [AvaloniaFact]
    public void GetImage_CachesResults()
    {
        var f = new ImageProviderTestContainer();
        f.Options = Opts;
        var service = f.GetRequiredService<AssetFolderImageProvider>();

        var result1 = service.GetImage(IntellisenseHint.Csv);
        var result2 = service.GetImage(IntellisenseHint.Csv);

        result1.Should().Be(result2).And.NotBeOfType<NullImage>();
    }

    [AvaloniaFact(Skip = "TODO")]
    public async Task GetImage_HandlesConcurrentAccess()
    {
        var f = new ImageProviderTestContainer();
        f.Options = Opts;
        var service = f.GetRequiredService<AssetFolderImageProvider>();

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => service.GetImage(IntellisenseHint.Csv)));

        var results = await Task.WhenAll(tasks);

        results.Should().AllBeOfType<SvgImage>();
    }
}

[ServiceProvider]
[Import<IAutocompletionModule>]
[Import<ILoggingModule>]
[Singleton<AppOptions>(Instance = nameof(Options))]
[Singleton<AssetFolderImageProvider>]
public partial class ImageProviderTestContainer
{
    public AppOptions Options { get; set; } = null!;
}
