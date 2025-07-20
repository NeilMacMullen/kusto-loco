using Intellisense.FileSystem.Paths;

namespace lokqlDxComponents.Models;

public class Asset
{
    private readonly string _resourcePath = string.Empty;

    public string ResourcePath
    {
        get => _resourcePath;
        init => _resourcePath = value.NormalizePath();
    }

    public string AssemblyName { get; init; } = string.Empty;
    public string Folder => Path.GetDirectoryName(ResourcePath) ?? "";
    public bool IsEmpty => ResourcePath.Length is 0;
    public static readonly Asset Empty = new();
}
