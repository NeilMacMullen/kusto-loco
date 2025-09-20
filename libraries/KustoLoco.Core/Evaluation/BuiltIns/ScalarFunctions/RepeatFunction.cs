using System;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Repeat")]
internal partial class RepeatFunction
{
    public JsonNode? BoolImpl(bool s, long count) => RepeatCore.Repeat(s, count);
    public JsonNode? IntImpl(int s, long count) => RepeatCore.Repeat(s, count);
    public JsonNode? LongImpl(long s, long count) => RepeatCore.Repeat(s, count);
    public JsonNode? DecimalImpl(decimal s, long count) => RepeatCore.Repeat(s, count);
    public JsonNode? DoubleImpl(double s, long count) => RepeatCore.Repeat(s, count);
    public JsonNode? DateTimeImpl(DateTime s, long count) => RepeatCore.Repeat(s, count);
    public JsonNode? TimespanImpl(TimeSpan s, long count) => RepeatCore.Repeat(s, count);
    public JsonNode? GuidImpl(Guid s, long count) => RepeatCore.Repeat(s, count);
    public JsonNode? StringImpl(string s,long count) => RepeatCore.Repeat(s, count);
}

internal static class RepeatCore
{
    public static JsonNode? Repeat<T>(T value, long count)
    {
        if (count < 0)
            return null;
        if (count == 0) return new JsonArray();

        var nodes = new JsonNode?[count];
        for (var i = 0; i < count; i++)
            nodes[i] = JsonValue.Create<T>(value);
        return new JsonArray(nodes);
    }
}
