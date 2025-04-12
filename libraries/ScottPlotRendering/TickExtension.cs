using ScottPlot;

namespace KustoLoco.Rendering.ScottPlot;

public static class TickExtension
{
    public static Tick[] ToMajorTicks(this IEnumerable<GenericTick> genericTicks)
    {
        return genericTicks.Select(kv => new Tick(kv.Value, kv.Name, true)).ToArray();
    }

    public static Tick[] ToMinorTicks(this IEnumerable<GenericTick> genericTicks)
    {
        return genericTicks.Select(kv => new Tick(kv.Value, kv.Name, false)).ToArray();
    }
}
