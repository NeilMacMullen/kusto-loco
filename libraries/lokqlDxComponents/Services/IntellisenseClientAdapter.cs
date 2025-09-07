using System.CommandLine.Parsing;
using Intellisense;
using Lokql.Engine;
using lokqlDxComponents.Services.Assets;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Services;

public class IntellisenseClientAdapter(
    IntellisenseClient intellisenseClient,
    ILogger<IntellisenseClientAdapter> logger,
    IImageProvider imageProvider
    )
{
    private Dictionary<string, HashSet<string>> AllowedCommandsAndExtensions { get; set; } = [];
    public IntellisenseEntry[] InternalCommands { get; private set; } = [];

    public IImageProvider _imageProvider => imageProvider;

    public void SetInternalCommands(IEnumerable<VerbEntry> verbEntries)
    {
        var verbs = verbEntries.ToArray();
        var lookup = CreateLookup(verbs);
        var internalCommands = verbs.Select(x => new IntellisenseEntry(x.Name, x.HelpText, string.Empty,
            IntellisenseHint.Command)).ToArray();
        AllowedCommandsAndExtensions = lookup;
        InternalCommands = internalCommands;
    }


    public async Task OnCaretPositionChanged() => await intellisenseClient.CancelRequestAsync();

    public async Task<CompletionResult> GetPathCompletions(string currentLineText)
    {
        var args = CommandLineStringSplitter.Instance.Split(currentLineText).ToArray();

        if (args.Length < 2)
        {
            return CompletionResult.Empty;
        }

        var lastArg = args[^1];
        var command = args[0];

        // check if it starts with a valid file IO command like ".save"
        if (!AllowedCommandsAndExtensions.TryGetValue(command, out var extensions))
        {
            return CompletionResult.Empty;
        }


        var result = CompletionResult.Empty;
        try
        {
            // TODO: discreetly notify user (status bar? notifications inbox?) to check connection status of saved connections
            // and user profile app was started with if hosts don't show shares
            result = await intellisenseClient.GetCompletionResultAsync(lastArg);
        }
        catch (IntellisenseException exc)
        {
            logger.LogWarning(exc, "Intellisense exception occurred");
        }
        catch (OperationCanceledException exc)
        {
            logger.LogDebug(exc, "Intellisense request cancelled");
        }

        if (extensions.Count > 0 && !result.IsEmpty())
        {
            return result with
            {
                Entries = result
                    .Entries
                    .Where(x =>
                        {
                            // permit folders (which do not have extensions). note that files without extensions will still be allowed
                            var ext = Path.GetExtension(x.Name);
                            return ext == string.Empty || extensions.Contains(ext);
                        }
                    )
                    .ToList()
            };
        }

        return result;
    }

    private static Dictionary<string, HashSet<string>> CreateLookup(IEnumerable<VerbEntry> verbs)
    {
        var fileIoCommands = verbs.Where(x => x.SupportsFiles);
        var comparer = StringComparer.OrdinalIgnoreCase;
        return fileIoCommands.ToDictionary(
            x => "." + x.Name,
            x => x.SupportedExtensions.ToHashSet(comparer),
            comparer
        );
    }
}
