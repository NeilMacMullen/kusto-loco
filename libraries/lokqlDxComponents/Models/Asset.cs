namespace lokqlDxComponents.Models;

public interface IAsset
{
    public static readonly IAsset Empty = new EmptyAsset();
    string ResourcePath { get; }
    string Directory { get; }
    bool IsEmpty { get; }
}

public class Asset : IAsset
{
    private string? _resourcePath;
    private string? _directory;
    public Uri Uri { get; }

    private Asset(Uri uri)
    {
        Uri = uri;
    }

    public static Asset Create(string uri) => Create(new Uri(uri));
    public static Asset Create(Uri uri) => new(uri);

    public string AbsolutePath => Uri.AbsolutePath;

    public string BaseDirectory => Uri.Segments.Length switch
    {
        0 => string.Empty,
        <= 2 => Uri.Segments[0],
        _ => Uri.Segments[1]
    };

    public string ResourcePath => _resourcePath ??= Path.GetRelativePath($"/{BaseDirectory}", AbsolutePath);
    public string Directory => _directory ??= Path.GetDirectoryName(ResourcePath) ?? "";
    public bool IsEmpty => false;
    public string AssemblyName => Uri.Authority;
    public string BaseUri => Uri.GetLeftPart(UriPartial.Authority);
}

file class EmptyAsset : IAsset
{
    public string ResourcePath => string.Empty;
    public string Directory => string.Empty;
    public bool IsEmpty => true;
}
