using System;
using System.IO;
using NotNullStrings;
using static SimpleExec.Command;

namespace IntellisenseTests;

public sealed class TestShare : IDisposable
{

    private TestShare(){}

    public DirectoryInfo Folder { get; set; } = null!;
    public string Name { get; set; } = null!;

    public static TestShare Create(string name = "")
    {
        var share = new TestShare();
        if (name.IsBlank())
        {
            var id = Guid.NewGuid();
            share.Name = $"TestShare_{id}";
        }
        else
        {
            share.Name = name;
        }

        var shareFolder = Path.Combine(Path.GetTempPath(), share.Name);
        share.Folder = Directory.CreateDirectory(shareFolder);

        try
        {
            Run("net",
                $"share {share.Name}=\"{share.Folder.FullName}\" /GRANT:Everyone,FULL",
                noEcho: true
            );
        }
        catch (Exception ex)
        {
            share.Dispose();
            throw new Exception($"Failed to create share {share.Name}", ex);
        }

        return share;
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
