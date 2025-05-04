namespace Intellisense.FileSystem;

internal static class FileSystemExtensions
{
    public static void Touch(this FileInfo file)
    {
        if (file.Directory is not { } dir)
        {
            dir = new DirectoryInfo(Path.GetTempPath());
            file = new FileInfo(Path.Combine(dir.FullName, file.Name));
        }

        if (!dir.Exists)
        {
            dir.Create();
        }

        file.Create().Dispose();
    }

    public static List<FileInfo> TouchFiles(this DirectoryInfo directory, IEnumerable<string> filePaths) =>
        filePaths.Select(directory.TouchFile).ToList();

    public static FileInfo TouchFile(this DirectoryInfo directory, string fileName)
    {
        if (!directory.Exists)
        {
            directory.Create();
        }
        var path = Path.Combine(directory.FullName, fileName);
        var file = new FileInfo(path);
        file.Touch();
        return file;
    }

    public static void CreateOrClean(this DirectoryInfo directory)
    {
        if (directory.Exists)
        {
            directory.Delete(true);
        }
        directory.Create();
    }
}
