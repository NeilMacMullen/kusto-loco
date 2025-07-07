using System.IO.Abstractions.TestingHelpers;
using Avalonia.Headless.XUnit;
using Avalonia.Svg;
using AwesomeAssertions;
using Jab;
using LogSetup;
using lokqlDxComponents.Configuration;
using lokqlDxComponents.Services;
using Microsoft.Extensions.DependencyInjection;

namespace lokqlDxComponentsTests;

public class AssetFolderImageServiceTests
{

    private const int Json = 10;
    private const int Csv = 20;
    private const int File = 30;
    private const int Folder = 40;


    private static readonly ImageServiceOptions Opts = new()
    {
        AssetsFolder = new($"avares://{nameof(lokqlDxComponentsTests)}/Assets/FileIcons/"),
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

        var service = f.GetRequiredService<AssetFolderImageService>();

        var imgSource = service.GetImageSource(file);
        var img = service.GetImage(imgSource);
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

        var service = f.GetRequiredService<AssetFolderImageService>();

        var imgSource = service.GetImageSource(file);
        var img = service.GetImage(imgSource);
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

        var service = f.GetRequiredService<AssetFolderImageService>();

        var imgSource = service.GetImageSource(dir);
        var img = service.GetImage(imgSource);
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

        var service = f.GetRequiredService<AssetFolderImageService>();
        var imgSource = service.GetImageSource(file);
        imgSource.Segments[^1].Should().Be("csv.svg");
    }
}

[ServiceProvider]
[Import<IAutocompletionModule>]
[Import<ILoggingModule>]
[Singleton<ImageServiceOptions>(Instance = nameof(Options))]
[Singleton<AssetFolderImageService>]
public partial class ImageProviderTestContainer
{
    public ImageServiceOptions Options { get; set; } = null!;
}
