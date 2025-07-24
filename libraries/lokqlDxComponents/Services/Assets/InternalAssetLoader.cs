using Avalonia.Platform;

namespace lokqlDxComponents.Services.Assets;

public interface IInternalAssetLoader
{
    IEnumerable<Uri> GetAssets(Uri uri);
    bool Exists(Uri uri);
    Stream Open(Uri uri);
}


public class InternalAssetLoader : IInternalAssetLoader
{
    public IEnumerable<Uri> GetAssets(Uri uri) => AssetLoader.GetAssets(uri, null);
    public bool Exists(Uri uri) => AssetLoader.Exists(uri);
    public Stream Open(Uri uri) => AssetLoader.Open(uri);
}
