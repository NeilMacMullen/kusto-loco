namespace Intellisense.FileSystem.Paths;

public abstract class FileSystemPath
{
    private readonly string _value;

    internal FileSystemPath(string value)
    {
        _value = value;
    }
    public virtual string Value => _value;
    public abstract bool IsRootDirectory { get; }
    public virtual string ParentPath => Path.GetDirectoryName(_value) ?? string.Empty;
    public virtual bool EndsWithDirectorySeparator => _value.EndsWithDirectorySeparator();
    public virtual bool IsEmpty => this is EmptyPath;
    public static FileSystemPath Empty => EmptyPath.Instance;
}