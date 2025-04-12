using System.CommandLine.Parsing;

namespace Intellisense;

public class CommandParser
{
    private readonly HashSet<string> _supportedCommands;

    public CommandParser(IEnumerable<string> supportedCommands, string prefix)
    {
        _supportedCommands = supportedCommands
            .Select(x => prefix + x)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public string GetLastArgument(string text)
    {
        var args = CommandLineStringSplitter.Instance.Split(text).ToArray();

        if (args.Length < 2)
        {
            return string.Empty;
        }

        if (!_supportedCommands.Contains(args[0]))
        {
            return string.Empty;
        }

        return args[^1];
    }
}
