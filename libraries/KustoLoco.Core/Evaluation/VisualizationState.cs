using System.Collections.Immutable;

namespace KustoLoco.Core.Evaluation;

public record VisualizationState(string ChartType, ImmutableDictionary<string, string> Properties)
{
    public static readonly VisualizationState Empty =
        new(string.Empty, ImmutableDictionary<string, string>.Empty);

    public string PropertyOr(string propertyName, string fallback)
        => Properties.TryGetValue(propertyName, out var v) ? v : fallback;
}
