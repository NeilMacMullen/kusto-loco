//
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text;

namespace KustoLoco.UserAgent;

/// <summary>
/// A minimal reader for the ua-parser <c>regexes.yaml</c> layout, which uses a strict, regular subset of YAML:
/// three top-level sections (at column 0), each a list of entries introduced by <c>- regex:</c> with optional
/// single-quoted <c>key: value</c> lines. This avoids taking a general YAML dependency for a fixed, known format.
/// (Verified against the dataset: every value is single-quoted, single-line; '' denotes an escaped quote.)
/// </summary>
internal static class RegexesYaml
{
    internal sealed class Entry
    {
        private readonly Dictionary<string, string> _map = new(System.StringComparer.Ordinal);
        public void Set(string key, string value) => _map[key] = value;
        public string? Get(string key) => _map.TryGetValue(key, out var v) ? v : null;
    }

    public static Dictionary<string, List<Entry>> Parse(string text)
    {
        var sections = new Dictionary<string, List<Entry>>(System.StringComparer.Ordinal);
        List<Entry>? current = null;
        Entry? entry = null;

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            if (line.Length == 0)
                continue;

            // Section header: content at column 0, ending in ':'.
            if (line[0] != ' ' && line[0] != '\t' && line[0] != '#')
            {
                var header = line.TrimEnd();
                if (header.EndsWith(":", System.StringComparison.Ordinal))
                {
                    var name = header[..^1].Trim();
                    current = new List<Entry>();
                    sections[name] = current;
                    entry = null;
                    continue;
                }
            }

            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed[0] == '#' || current is null)
                continue;

            if (trimmed.StartsWith("- ", System.StringComparison.Ordinal))
            {
                entry = new Entry();
                current.Add(entry);
                AddKeyValue(entry, trimmed[2..]);
            }
            else if (entry is not null)
            {
                AddKeyValue(entry, trimmed);
            }
        }

        return sections;
    }

    private static void AddKeyValue(Entry entry, string keyValue)
    {
        var colon = keyValue.IndexOf(':');
        if (colon <= 0)
            return;
        var key = keyValue[..colon].Trim();
        var value = ParseSingleQuoted(keyValue[(colon + 1)..].Trim());
        if (key.Length > 0)
            entry.Set(key, value);
    }

    // Single-quoted YAML scalar with '' as an escaped quote; anything unquoted is returned verbatim.
    private static string ParseSingleQuoted(string s)
    {
        if (s.Length == 0 || s[0] != '\'')
            return s;
        var sb = new StringBuilder(s.Length);
        for (var i = 1; i < s.Length; i++)
        {
            if (s[i] == '\'')
            {
                if (i + 1 < s.Length && s[i + 1] == '\'') { sb.Append('\''); i++; }
                else break; // closing quote
            }
            else
            {
                sb.Append(s[i]);
            }
        }
        return sb.ToString();
    }
}
