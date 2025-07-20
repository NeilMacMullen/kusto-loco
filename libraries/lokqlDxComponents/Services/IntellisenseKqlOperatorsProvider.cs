using CommunityToolkit.Mvvm.Messaging;
using Intellisense;
using lokqlDxComponents.Contexts;
using lokqlDxComponents.Events;
using lokqlDxComponents.Models;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Services;

public interface IIntellisenseKqlOperatorsProvider
{
    IReadOnlyList<IntellisenseEntry> GetKqlOperators();
}

public class IntellisenseKqlOperatorsProvider : IIntellisenseKqlOperatorsProvider, IRecipient<InitMessage>
{
    private readonly IQueryEditorContext _ctx;
    private readonly IAssetService _assetService;
    private readonly ILogger<IntellisenseKqlOperatorsProvider> _logger;
    private IntellisenseEntry[] _kqlOperatorEntries = [];

    public IntellisenseKqlOperatorsProvider(
        IQueryEditorContext ctx,
        IAssetService assetService,
        ILogger<IntellisenseKqlOperatorsProvider> logger)
    {
        _ctx = ctx;
        _assetService = assetService;
        _logger = logger;
        _ctx.Messenger.RegisterAll(this);
    }

    public IReadOnlyList<IntellisenseEntry> GetKqlOperators() => _kqlOperatorEntries;

    private void LoadOperators()
    {
        _kqlOperatorEntries = _assetService.Deserialize<IntellisenseEntry[]>(AssetLocations.IntellisenseOperators)
            .Select(i => i with { Hint = IntellisenseHint.Operator })
            .ToArray();
        _logger.LogInformation("Loaded {KqlOperatorCount} operators", _kqlOperatorEntries.Length);
    }

    public void Receive(InitMessage message)
    {
        using var _ = _ctx.BeginLoggingScope();
        LoadOperators();
    }
}
