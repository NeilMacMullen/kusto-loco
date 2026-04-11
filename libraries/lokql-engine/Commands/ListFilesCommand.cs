using System.Collections.Immutable;
using CommandLine;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.PluginSupport;
using NotNullStrings;

namespace Lokql.Engine.Commands;

/// <summary>
///     List data files in a directory
/// </summary>
public static class ListFilesCommand
{
    internal static Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var settings = context.Settings;
        var queryContext = context.QueryContext;

        // Determine the search path
        var searchPath = o.Path.IsNotBlank() 
            ? o.Path 
            : settings.Get(StandardFormatAdaptor.Settings.KustoDataPath);

        var files = GetFiles(searchPath, console);

        if (files.Length == 0)
        {
            console.Warn($"No data files found in {searchPath}");
            return Task.CompletedTask;
        }

        const string tableName = "_files";
        var table = TableBuilder.CreateFromImmutableData(tableName, files);
        queryContext.AddTable(table);
        context.RunInput(tableName);
        console.Info($"Found {files.Length} file(s) - results available as table {tableName}");
        return Task.CompletedTask;
    }

    private static ImmutableArray<FileInfo> GetFiles(string searchPath, IKustoConsole console)
    {
        try
        {
            // Check if the path contains wildcards
            var directory = Path.GetDirectoryName(searchPath);
            var pattern = Path.GetFileName(searchPath);

            // If no wildcards and path is a directory, use *.* pattern
            if (string.IsNullOrEmpty(pattern) || (!pattern.Contains('*') && !pattern.Contains('?')))
            {
                if (Directory.Exists(searchPath))
                {
                    directory = searchPath;
                    pattern = "*.*";
                }
                else if (string.IsNullOrEmpty(directory))
                {
                    directory = searchPath;
                    pattern = "*.*";
                }
            }

            // Default to current directory if none specified
            if (string.IsNullOrEmpty(directory))
                directory = ".";

            if (!Directory.Exists(directory))
            {
                console.Warn($"Directory not found: {directory}");
                return ImmutableArray<FileInfo>.Empty;
            }

            var dirInfo = new DirectoryInfo(directory);
            var foundFiles = dirInfo.GetFiles(pattern)
                .Select(f => new FileInfo(
                    Name: System.IO.Path.GetFileNameWithoutExtension(f.Name),
                    Path: f.FullName,
                    Type: f.Extension.TrimStart('.'),
                    Size: FormatSize(f.Length),
                    LastModified: f.LastWriteTime
                ))
                .OrderBy(f => f.Name)
                .ToImmutableArray();

            return foundFiles;
        }
        catch (Exception ex)
        {
            console.Error($"Error listing files: {ex.Message}");
            return ImmutableArray<FileInfo>.Empty;
        }
    }

    private static string FormatSize(long bytes)
    {
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;

        if (bytes >= GB)
            return $"{Math.Round(bytes / (double)GB, 2)} GB";
        if (bytes >= MB)
            return $"{Math.Round(bytes / (double)MB, 2)} MB";
        if (bytes >= KB)
            return $"{Math.Round(bytes / (double)KB, 2)} KB";
        return $"{bytes} B";
    }

    [Verb("lsfiles", aliases: ["dir", "listfiles"],
        HelpText = @"Lists data files in a directory with their metadata
By default lists files in the current kusto.datapath directory.
Supports wildcards (* and ?) in the file pattern.
Examples:
  .lsfiles                    # List all files in kusto.datapath
  .lsfiles C:\data            # List all files in C:\data
  .lsfiles C:\data\*.csv      # List all CSV files in C:\data
  .lsfiles *.parquet          # List all parquet files in kusto.datapath")]
    internal class Options
    {
        [Value(0, HelpText = "Directory path or file pattern (supports wildcards)", Required = false)]
        [FileOptions]
        public string Path { get; set; } = string.Empty;
    }

    private record FileInfo(
        string Name,
        string Path,
        string Type,
        string Size,
        DateTime LastModified
    );
}
