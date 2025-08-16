using System;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;

namespace KustoLoco.Core.Settings;

public readonly record struct KustoSettingDefinition(string Name, string Description, string DefaultValue, string Type)
{
    public static KustoSettingDefinition FromName(string name)
        => new KustoSettingDefinition(name, String.Empty, String.Empty, String.Empty);
}
