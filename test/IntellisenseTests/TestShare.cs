using System;
using System.IO;
using static SimpleExec.Command;

namespace IntellisenseTests;

public sealed class TestShare : IDisposable
{
    public DirectoryInfo Folder { get; }
    public string Name { get; }

    public TestShare()
    {
        var id = Guid.NewGuid();
        Name = $"TestShare_{id}";
        var shareFolder = Path.Combine(Path.GetTempPath(), Name);
        Folder = Directory.CreateDirectory(shareFolder);

        try
        {
            Run("net",
                $"share {Name}=\"{Folder.FullName}\" /GRANT:Everyone,FULL",
                noEcho: true
            );
        }
        catch (Exception ex)
        {
            Dispose();
            throw new Exception($"Failed to create share {Name}", ex);
        }
    }

    public void Dispose()
    {
        try
        {
            Run("net", $"share {Name} /DELETE /Y", noEcho: true);
        }
        finally
        {
            Folder.Delete(recursive: true);
        }
    }
}
