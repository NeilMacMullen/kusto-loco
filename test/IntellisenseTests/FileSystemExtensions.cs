using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace IntellisenseTests;

internal static class FileSystemExtensions
{
    public static void Touch(this IFileInfo file)
    {
        if (!file.Directory!.Exists)
        {
            file.Directory.Create();
        }
        file.Create().Dispose();
    }

    public static void SetContent(this IFileInfo file, string content)
    {
        file.EnsureDeleted();
        file.EnsureParentExists();
        file.FileSystem.File.WriteAllText(file.FullName, content);
    }

    public static void EnsureParentExists(this IFileInfo file)
    {
        if (!file.Directory!.Exists)
        {
            file.Directory.Create();
        }
    }

    public static List<IFileInfo> TouchFiles(this IDirectoryInfo directory, IEnumerable<string> filePaths) =>
        filePaths.Select(directory.TouchFile).ToList();

    public static IFileInfo TouchFile(this IDirectoryInfo directory, string fileName)
    {
        var path2 = directory.FileSystem.Path;
        if (!directory.Exists)
        {
            directory.Create();
        }
        var path = path2.Combine(directory.FullName, fileName);
        var file = directory.FileSystem.FileInfo.New(path);
        file.Touch();
        return file;
    }

    public static void CreateOrClean(this IDirectoryInfo directory)
    {
        if (directory.Exists)
        {
            directory.Delete(true);
        }
        directory.Create();
    }

    public static void EnsureDeleted(this IFileInfo file)
    {
        try
        {
            file.Delete();
        }
        catch (Exception)
        {
            // ignore
        }

    }
}
