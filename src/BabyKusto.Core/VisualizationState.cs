using System.Collections.Immutable;

namespace BabyKusto.Core.Evaluation;

public record VisualizationState(string ChartType, ImmutableDictionary<string, object> Items)
{
    public static readonly VisualizationState Empty =
        new(string.Empty, ImmutableDictionary<string, object>.Empty);
}