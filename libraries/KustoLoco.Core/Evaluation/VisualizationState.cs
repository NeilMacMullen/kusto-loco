using System.Collections.Immutable;

namespace KustoLoco.Core.Evaluation;

public record VisualizationState(string ChartType, ImmutableDictionary<string, object> Items)
{
    public static readonly VisualizationState Empty =
        new(string.Empty, ImmutableDictionary<string, object>.Empty);

    public string PropertyOr(string propertyName, string fallback)
        => Items.TryGetValue(propertyName, out var v) ? v.ToString() ?? fallback : fallback;
}