using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// extract_all(regex, source) / extract_all(regex, captureGroups, source) -> dynamic. ADX shape: 0 capture groups ->
// the full matches; 1 group -> that group per match; >1 groups (or an explicit captureGroups list) -> an array of
// arrays. A bad pattern or a match-timeout (ReDoS guard) -> null; no match -> an empty array.
[KustoImplementation(Keyword = "Functions.ExtractAll")]
internal partial class ExtractAllFunction
{
    private static JsonNode? Impl(string regex, string source) => ExtractAllSupport.Extract(regex, source, null);

    private static JsonNode? GroupsImpl(string regex, JsonNode captureGroups, string source)
        => ExtractAllSupport.Extract(regex, source, captureGroups);
}

// Separate class: the source generator inlines an Impl body, so an Impl must not call a private same-class helper.
internal static class ExtractAllSupport
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    public static JsonNode? Extract(string? pattern, string? source, JsonNode? captureGroups)
    {
        if (string.IsNullOrEmpty(pattern) || source is null) return null;
        Regex rx;
        try { rx = new Regex(pattern, RegexOptions.CultureInvariant, RegexTimeout); }
        catch (ArgumentException) { return null; }

        var matches = new List<Match>();
        try { foreach (Match m in rx.Matches(source)) matches.Add(m); }
        catch (RegexMatchTimeoutException) { return null; }
        if (matches.Count == 0) return new JsonArray();

        if (captureGroups is JsonArray wantedArr)
        {
            var wanted = new List<int>();
            foreach (var el in wantedArr)
            {
                if (el is JsonValue v && v.TryGetValue<long>(out var l)) wanted.Add((int)l);
                else if (el is JsonValue vs && vs.TryGetValue<string>(out var s) && int.TryParse(s, out var pi)) wanted.Add(pi);
                else return null;
            }
            var outer = new JsonArray();
            foreach (var m in matches)
            {
                var row = new JsonArray();
                foreach (var g in wanted)
                    row.Add(g >= 0 && g < m.Groups.Count && m.Groups[g].Success ? JsonValue.Create(m.Groups[g].Value) : null);
                outer.Add(row);
            }
            return outer;
        }

        var groupCount = rx.GetGroupNumbers().Length - 1;   // minus the implicit whole-match group 0
        var result = new JsonArray();
        if (groupCount <= 0)
            foreach (var m in matches) result.Add(JsonValue.Create(m.Value));
        else if (groupCount == 1)
            foreach (var m in matches) result.Add(m.Groups[1].Success ? JsonValue.Create(m.Groups[1].Value) : null);
        else
            foreach (var m in matches)
            {
                var row = new JsonArray();
                for (var g = 1; g <= groupCount; g++)
                    row.Add(m.Groups[g].Success ? JsonValue.Create(m.Groups[g].Value) : null);
                result.Add(row);
            }
        return result;
    }
}
