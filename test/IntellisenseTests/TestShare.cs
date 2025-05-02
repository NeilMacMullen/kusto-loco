using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace IntellisenseTests;

public class TestShare : IDisposable
{
    public DirectoryInfo Folder { get; set; }
    public string Name { get; set; }

    public TestShare()
    {
        // initialize folder
        var id = Guid.NewGuid();
        var shareName = $"TestShare_{id}";
        var shareFolder = Path.Combine(Path.GetTempPath(), shareName);
        var folder = Directory.CreateDirectory(shareFolder);
        Name = shareName;
        Folder = folder;

        try
        {
            // initialize shares
            using var ps = PowerShell.Create();
            ps
                .AddCommand("New-SmbShare")
                .AddParameter("Name", Name)
                .AddParameter("Path", Folder.FullName)
                .AddParameter("FullAccess", "Everyone")
                .Invoke();

            if (ps.HadErrors)
            {
                var errs = GetErrors(ps);
                throw new AggregateException(errs);
            }
        }
        catch (Exception)
        {
            Dispose();
            throw;
        }
    }

    private static IEnumerable<Exception> GetErrors(PowerShell ps)
    {
        var errs = ps.Streams.Error.Select(x =>
            {
                if (x.CategoryInfo.Category is ErrorCategory.PermissionDenied)
                {
                    const string message =
                        "Insufficient permissions. Did the executing session have administrator rights?";
                    return new PSSecurityException(message, x.Exception);
                }

                if (x.Exception is null)
                {
                    return new Exception(x.ToString())
                    {
                        Data = { ["ErrorRecord"] = x }
                    };
                }

                return x.Exception;
            }
        );
        return errs;
    }


    public void Dispose()
    {
        try
        {
            using var ps = PowerShell
                .Create()
                .AddCommand("Remove-SmbShare")
                .AddParameter("Name", Name)
                .AddParameter("Force", true);
            ps.Invoke();
        }
        finally
        {
            Folder.Delete(true);
        }
    }
}
