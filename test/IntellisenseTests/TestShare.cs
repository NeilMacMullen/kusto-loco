using System;
using System.IO.Abstractions;
using NotNullStrings;
using static SimpleExec.Command;

namespace IntellisenseTests;

public sealed class TestShare : IDisposable
{

    private TestShare(){}

    public IDirectoryInfo Folder { get; set; } = null!;
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


        share.Folder = TestHelper.CreateCleanTestDirectory(share.Name);

        try
        {
            Run("net",
                $"share {share.Name}=\"{share.Folder.FullName}\" /GRANT:Everyone,FULL"
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
            Run("net", $"share {Name} /DELETE /Y");

        }
        finally
        {
            Folder.Delete(recursive: true);
        }
    }
}
