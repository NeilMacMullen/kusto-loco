using Avalonia.Media;

namespace lokqlDxComponents.Services;

public interface IInternalAssetLoader
{
    IImage LoadImage(Uri uri);
    IEnumerable<Uri> GetAssets(Uri assetFolder);
}
