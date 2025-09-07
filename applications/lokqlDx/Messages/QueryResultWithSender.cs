using KustoLoco.Core;
using KustoLoco.Core.Settings;

public readonly record struct QueryResultWithSender(string Sender,
    KustoQueryResult Result,
    KustoSettingsProvider Settings,
    bool ImmediateDisplay);
