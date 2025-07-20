using CommunityToolkit.Mvvm.Messaging;
using Intellisense;
using Lokql.Engine;
using lokqlDxComponents.Contexts;
using lokqlDxComponents.Events;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Services;

public interface IIntellisenseCommandsProvider
{
    IReadOnlyList<IntellisenseEntry> GetInternalCommands();
    Dictionary<string, HashSet<string>> GetAllowedCommandsAndExtensions();
}

public class IntellisenseCommandsProvider : IIntellisenseCommandsProvider, IRecipient<InitMessage>
{
    private readonly IQueryEditorContext _ctx;
    private readonly ILogger<IntellisenseCommandsProvider> _logger;
    private IntellisenseEntry[] _internalCommands = [];
    private Dictionary<string, HashSet<string>> _allowedCommandsAndExtensions = [];

    public IntellisenseCommandsProvider(IQueryEditorContext ctx, ILogger<IntellisenseCommandsProvider> logger)
    {
        _ctx = ctx;
        _logger = logger;
        _ctx.Messenger.RegisterAll(this);
    }



    public IReadOnlyList<IntellisenseEntry> GetInternalCommands() => _internalCommands;

    public Dictionary<string, HashSet<string>> GetAllowedCommandsAndExtensions() => _allowedCommandsAndExtensions;

    private void AddInternalCommands(IEnumerable<VerbEntry> verbEntries)
    {
        var verbs = verbEntries.ToArray();
        var lookup = CreateLookup(verbs);
        var internalCommands = verbs
            .Select(x => new IntellisenseEntry(x.Name,
                    x.HelpText,
                    string.Empty,
                    IntellisenseHint.Command
                )
            )
            .ToArray();
        _allowedCommandsAndExtensions = lookup;
        _internalCommands = internalCommands;
        _logger.LogInformation("Initialized command resources. {InternalCommandsCount} {AllowedCommandsAndExtensionsCount}",
            _internalCommands.Length,
            _allowedCommandsAndExtensions.Count
        );
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

    public void Receive(InitMessage message)
    {
        using var _ = _ctx.BeginLoggingScope();
        AddInternalCommands(_ctx.QueryEngineContext.GetVerbs());
    }
}
