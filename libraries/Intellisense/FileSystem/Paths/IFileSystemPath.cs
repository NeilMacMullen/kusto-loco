namespace Intellisense.FileSystem.Paths;

public abstract class FileSystemPath(string value)
{
    public virtual string Value => value;
    public abstract bool IsRootDirectory { get; }
    public virtual string ParentPath => Path.GetDirectoryName(value) ?? string.Empty;
    public virtual bool EndsWithDirectorySeparator => value.EndsWithDirectorySeparator();
    public virtual bool IsEmpty => this is EmptyPath;
}