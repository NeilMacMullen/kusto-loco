using CommunityToolkit.Mvvm.Messaging;
using Intellisense;
using lokqlDxComponents.Contexts;
using lokqlDxComponents.Events;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Services;

public interface IIntellisenseKqlFunctionsProvider
{
    IReadOnlyList<IntellisenseEntry> GetKqlFunctions();
}

public class IntellisenseKqlFunctionsProvider : IIntellisenseKqlFunctionsProvider, IRecipient<InitMessage>
{
    private readonly IQueryEditorContext _ctx;
    private readonly IAssetService _assetService;
    private readonly ILogger<IntellisenseKqlFunctionsProvider> _logger;
    private IntellisenseEntry[] _kqlFunctionEntries = [];

    public IntellisenseKqlFunctionsProvider(
        IQueryEditorContext ctx,
        IAssetService assetService,
        ILogger<IntellisenseKqlFunctionsProvider> logger)
    {
        _ctx = ctx;
        _assetService = assetService;
        _logger = logger;
        _ctx.Messenger.RegisterAll(this);
    }



    public IReadOnlyList<IntellisenseEntry> GetKqlFunctions() => _kqlFunctionEntries;

    private void LoadFunctions()
    {
        _kqlFunctionEntries = _assetService.Deserialize<IntellisenseEntry[]>(AssetLocations.IntellisenseFunctions)
            .Select(i => i with { Hint = IntellisenseHint.Function })
            .ToArray();
        _logger.LogInformation("Loaded {KqlFunctionCount} functions", _kqlFunctionEntries.Length);
    }

    public void Receive(InitMessage message)
    {
        using var _ = _ctx.BeginLoggingScope();
        LoadFunctions();
    }
}
