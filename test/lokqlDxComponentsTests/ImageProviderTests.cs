using System.IO.Abstractions.TestingHelpers;
using Avalonia.Headless.XUnit;
using Avalonia.Svg;
using AwesomeAssertions;
using Intellisense;
using Jab;
using LogSetup;
using lokqlDxComponents.Configuration;
using lokqlDxComponents.Services;
using Microsoft.Extensions.DependencyInjection;

namespace lokqlDxComponentsTests;

public class ImageProviderTests
{
    private const int Json = 10;
    private const int Csv = 20;
    private const int File = 30;
    private const int Folder = 40;


    private static readonly ImageServiceOptions Opts = new()
    {
        AssetsFolder = new($"avares://{nameof(lokqlDxComponentsTests)}/Assets/CompletionIcons/"),
        Extension = ".svg"
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

        var service = f.GetRequiredService<IImageProvider>();
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

        var service = f.GetRequiredService<IImageProvider>();
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

        var service = f.GetRequiredService<IImageProvider>();
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

        var service = f.GetRequiredService<IImageProvider>();
        var fileExtensionService = f.GetRequiredService<IFileExtensionService>();

        var img = service.GetImage(fileExtensionService.GetIntellisenseHint(dir));


        img.Should().BeOfType<SvgImage>().Which.Source.Picture!.CullRect.Width.Should().Be(Folder);
    }

    [AvaloniaFact]
    public void GetSource_DerivesSvgSourceFromBaseUriAndFileExtension()
    {
        var f = new ImageProviderTestContainer();
        f.Options = Opts;
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["/someFolder/myFolder/aFile.csv"] = new("")
            }
        );
        var file = fileSystem.AllFiles.Select(x => fileSystem.FileInfo.New(x)).Single();

        var fileExtensionService = f.GetRequiredService<IFileExtensionService>();

        var source = fileExtensionService.GetIntellisenseHint(file);

        source.Should().Be(IntellisenseHint.Csv);
    }
}

[ServiceProvider]
[Import<IAutocompletionModule>]
[Import<ILoggingModule>]
[Singleton<ImageServiceOptions>(Instance = nameof(Options))]
[Singleton<AssetFolderImageProvider>]
public partial class ImageProviderTestContainer
{
    public ImageServiceOptions Options { get; set; } = null!;
}
