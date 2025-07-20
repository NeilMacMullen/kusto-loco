using CommunityToolkit.Mvvm.Messaging;
using Intellisense;
using KustoLoco.Core.Settings;
using lokqlDxComponents.Contexts;
using lokqlDxComponents.Events;
using lokqlDxComponents.Models;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Services;

public interface IIntellisenseSettingsProvider
{
    IReadOnlyList<IntellisenseEntry> GetSettings();
}

public class IntellisenseSettingsProvider : IIntellisenseSettingsProvider, IRecipient<QueryExecutedMessage>, IRecipient<InitMessage>
{
    private readonly IQueryEditorContext _ctx;
    private readonly ILogger<IntellisenseSettingsProvider> _logger;
    private IntellisenseEntry[] _settingNames = [];

    public IntellisenseSettingsProvider(IQueryEditorContext ctx, ILogger<IntellisenseSettingsProvider> logger)
    {
        _ctx = ctx;
        _logger = logger;
        _ctx.Messenger.RegisterAll(this);
    }



    public IReadOnlyList<IntellisenseEntry> GetSettings() => _settingNames;

    private void AddSettingsForIntellisense(IEnumerable<RawKustoSetting> settings)
    {
        _settingNames = settings
            .Select(s => new IntellisenseEntry(s.Name, s.Value, string.Empty))
            .ToArray();

        _logger.LogInformation("Loaded {SettingsCount} settings", _settingNames.Length);
    }

    public void Receive(QueryExecutedMessage message)
    {
        using var _ = _ctx.BeginLoggingScope();

        _logger.LogInformation("Received {QueryExecutedMessage}", message);
        AddSettingsForIntellisense(_ctx.QueryEngineContext.GetSettings());
    }

    public void Receive(InitMessage message)
    {
        using var _ = _ctx.BeginLoggingScope();
        AddSettingsForIntellisense(_ctx.QueryEngineContext.GetSettings());
    }
}
