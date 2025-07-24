using System.IO.Abstractions.TestingHelpers;
using Avalonia.Headless.XUnit;
using Avalonia.Svg;
using AwesomeAssertions;
using Intellisense;
using Jab;
using LogSetup;
using lokqlDxComponents.Configuration;
using lokqlDxComponents.Services;
using lokqlDxComponents.Services.Assets;
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




    [AvaloniaTheory]
    [InlineData("json", Json)]
    [InlineData("csv", Csv)]
    public void GetImage_File_GetsImageAssociatedWithFileExtension(string fileExtension, int expectedWidth)
    {
        var f = new ImageProviderTestContainer();

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

    [AvaloniaTheory]
    [InlineData("txt", "pngFile")]
    [InlineData("xls", "jpgFile")]
    public void GetImage_SupportsRasterFormats(string extension, string fileName)
    {
        var f = new ImageProviderTestContainer();

        var fileStr = $"/someFolder/{fileName}.{extension}";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                [fileStr] = new("")
            }
        );
        var file = fileSystem.FileInfo.New(fileStr);

        var service = f.GetRequiredService<AssetFolderImageProvider>();
        var fileExtensionService = f.GetRequiredService<IFileExtensionService>();

        var result = service.GetImage(fileExtensionService.GetIntellisenseHint(file));

        result.Should().NotBeOfType<SvgImage>().And.NotBeOfType<NullImage>();
    }

    [AvaloniaFact]
    public void GetImage_SupportsSvgs()
    {
        var f = new ImageProviderTestContainer();

        var fileStr = "/someFolder/svgFile.csv";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                [fileStr] = new("")
            }
        );
        var file = fileSystem.FileInfo.New(fileStr);

        var service = f.GetRequiredService<AssetFolderImageProvider>();
        var fileExtensionService = f.GetRequiredService<IFileExtensionService>();

        var result = service.GetImage(fileExtensionService.GetIntellisenseHint(file));

        result.Should().BeOfType<SvgImage>();
    }


    [AvaloniaFact]
    public void GetImage_NonExistentAsset_GetsDefaultImage()
    {
        var f = new ImageProviderTestContainer();
        var service = f.GetRequiredService<AssetFolderImageProvider>();

        var result = service.GetImage(IntellisenseHint.Ppt);

        result.Should().BeOfType<NullImage>();
    }


}

[ServiceProvider]
[Import<IAutocompletionModule>]
[Import<ILoggingModule>]
[Singleton<IImageProvider>(Factory = nameof(GetAssetFolderImageProvider))]
[Singleton<AssetFolderImageProvider>]
public partial class ImageProviderTestContainer
{
    public IImageProvider GetAssetFolderImageProvider(AssetFolderImageProvider assetFolderImageProvider) => assetFolderImageProvider;

}
