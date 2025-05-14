using System.Collections.Generic;
using System.Collections.Immutable;
using NotNullStrings;

namespace KustoLoco.Core.Evaluation;

public record VisualizationState
{
    public static readonly VisualizationState Empty =
        new(string.Empty, ImmutableDictionary<string, string>.Empty);

    public VisualizationState(string chartType, ImmutableDictionary<string, string> properties)
    {
        ChartType = chartType;
        Properties = properties
            .ToImmutableDictionary(kv => kv.Key.ToLowerInvariant(),
                kv => kv.Value.NullToEmpty());
    }

    public string ChartType { get; init; }

    public ImmutableDictionary<string, string> Properties { get; init; }

    public string PropertyOr(string propertyName, string fallback) =>
        CollectionExtensions.GetValueOrDefault(Properties, propertyName.ToLowerInvariant(), fallback);
}
