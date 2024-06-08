using KustoLoco.Core.Settings;

namespace Lokql.Engine;

public static class LokqlSettings
{
    public static readonly KustoSettingDefinition ScriptPath = new("lokql.scriptpath",
        "location of script files", @"C:\kusto", nameof(String));

    public static readonly KustoSettingDefinition QueryPath = new("lokql.querypath",
        "location of query files", @"C:\kusto", nameof(String));

    public static void Register(KustoSettingsProvider settings)
    {
        settings.Register(ScriptPath);
        settings.Register(QueryPath);
    }
}
