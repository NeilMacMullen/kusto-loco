using System.CommandLine.Parsing;

namespace Intellisense.FileSystem;

public class FileIoCommandParser
{
    public string? Parse(string lineText)
    {
        var args = CommandLineStringSplitter.Instance.Split(lineText).ToList();
        if (args.Count < 2)
        {
            return null;
        }

        if (args[0] is not (".save" or ".load"))
        {
            return null;
        }

        var lastArg = args[^1];
        if (!Path.IsPathRooted(lastArg))
        {
            return null;
        }

        return lastArg;
    }
}
