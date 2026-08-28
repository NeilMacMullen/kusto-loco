//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using KustoLoco.Core;

namespace KustoLoco.UserAgent;

/// <summary>
/// An <see cref="IUserAgentParser"/> backed by the canonical <c>ua-parser/uap-core</c> dataset (Apache-2.0),
/// embedded in this package. KustoLoco's <c>parse_user_agent</c> function is native in the core engine; this
/// optional companion package supplies the faithful parsing data and cascade, so Core carries no user-agent
/// dependency (the plugin architecture of #613).
///
/// The uap-core algorithm is applied exactly: three ordered lists of regexes (user_agent / os / device); the
/// first matching regex in each list wins; replacement templates substitute capture groups (<c>$1..$9</c>), and
/// where a template is absent the positional capture group is used, defaulting the family to <c>"Other"</c>.
/// A regex that uses PCRE syntax .NET's engine rejects is skipped (see <see cref="SkippedRegexCount"/>) so the
/// remainder of the dataset still parses.
/// </summary>
public sealed class UapUserAgentParser : IUserAgentParser
{
    private readonly UaParser[] _browsers;
    private readonly OsParser[] _oses;
    private readonly DeviceParser[] _devices;

    /// <summary>A shared instance built from the embedded uap-core dataset.</summary>
    public static UapUserAgentParser Default { get; } = new();

    /// <summary>Build from the embedded uap-core <c>regexes.yaml</c>.</summary>
    public UapUserAgentParser() : this(ReadEmbeddedRegexes())
    {
    }

    /// <summary>Build from a caller-supplied uap-core <c>regexes.yaml</c> stream (e.g. a newer dataset).</summary>
    public UapUserAgentParser(Stream regexesYaml) : this(new StreamReader(regexesYaml).ReadToEnd())
    {
    }

    private UapUserAgentParser(string regexesYaml)
    {
        var sections = RegexesYaml.Parse(regexesYaml);
        var skipped = 0;
        _browsers = Build(sections, "user_agent_parsers", e => new UaParser(
            e.Regex, e.Get("family_replacement"), e.Get("v1_replacement"), e.Get("v2_replacement"), e.Get("v3_replacement")), ref skipped);
        _oses = Build(sections, "os_parsers", e => new OsParser(
            e.Regex, e.Get("os_replacement"), e.Get("os_v1_replacement"), e.Get("os_v2_replacement"),
            e.Get("os_v3_replacement"), e.Get("os_v4_replacement")), ref skipped);
        _devices = Build(sections, "device_parsers", e => new DeviceParser(
            e.Regex, e.Get("device_replacement"), e.Get("brand_replacement"), e.Get("model_replacement")), ref skipped);
        SkippedRegexCount = skipped;
    }

    /// <summary>Number of dataset entries whose regex .NET could not compile (PCRE-only syntax), and were skipped.</summary>
    public int SkippedRegexCount { get; }

    public UserAgentInfo Parse(string userAgent)
    {
        userAgent ??= string.Empty;
        return new UserAgentInfo(ParseBrowser(userAgent), ParseOs(userAgent), ParseDevice(userAgent));
    }

    private UserAgentSoftware ParseBrowser(string ua)
    {
        foreach (var p in _browsers)
        {
            var m = p.Regex.Match(ua);
            if (!m.Success) continue;
            return new UserAgentSoftware(
                Field(p.Family, m, 1) ?? "Other",
                Field(p.V1, m, 2), Field(p.V2, m, 3), Field(p.V3, m, 4));
        }
        return new UserAgentSoftware("Other");
    }

    private UserAgentSoftware ParseOs(string ua)
    {
        foreach (var p in _oses)
        {
            var m = p.Regex.Match(ua);
            if (!m.Success) continue;
            return new UserAgentSoftware(
                Field(p.Os, m, 1) ?? "Other",
                Field(p.V1, m, 2), Field(p.V2, m, 3), Field(p.V3, m, 4), Field(p.V4, m, 5));
        }
        return new UserAgentSoftware("Other");
    }

    private UserAgentDevice ParseDevice(string ua)
    {
        foreach (var p in _devices)
        {
            var m = p.Regex.Match(ua);
            if (!m.Success) continue;
            return new UserAgentDevice(
                Field(p.Device, m, 1) ?? "Other",
                p.Brand is null ? null : Empty(Substitute(p.Brand, m)),
                Field(p.Model, m, 1));
        }
        return new UserAgentDevice("Other");
    }

    // A field is: the substituted template if one is given (null when it substitutes to empty), else the
    // positional capture group (null when absent/empty). This is the uap-core replacement rule.
    private static string? Field(string? template, Match m, int defaultGroup)
    {
        if (template is not null)
            return Empty(Substitute(template, m));
        if (defaultGroup < m.Groups.Count && m.Groups[defaultGroup].Success)
            return Empty(m.Groups[defaultGroup].Value);
        return null;
    }

    private static string? Empty(string? s) => string.IsNullOrEmpty(s) ? null : s;

    private static string Substitute(string template, Match m)
    {
        if (!template.Contains('$'))
            return template.Trim();
        var sb = new StringBuilder(template.Length);
        for (var i = 0; i < template.Length; i++)
        {
            if (template[i] == '$' && i + 1 < template.Length && char.IsDigit(template[i + 1]))
            {
                var g = template[i + 1] - '0';
                if (g < m.Groups.Count && m.Groups[g].Success)
                    sb.Append(m.Groups[g].Value);
                i++;
            }
            else
            {
                sb.Append(template[i]);
            }
        }
        return sb.ToString().Trim();
    }

    private static T[] Build<T>(IReadOnlyDictionary<string, List<RegexesYaml.Entry>> sections, string section,
        Func<CompiledEntry, T> project, ref int skipped)
    {
        if (!sections.TryGetValue(section, out var entries))
            return Array.Empty<T>();
        var result = new List<T>(entries.Count);
        foreach (var e in entries)
        {
            var pattern = e.Get("regex");
            if (pattern is null) continue;
            var options = RegexOptions.CultureInvariant;
            if (e.Get("regex_flag") is { } flag && flag.Contains('i'))
                options |= RegexOptions.IgnoreCase;
            Regex regex;
            try
            {
                regex = new Regex(pattern, options);
            }
            catch (ArgumentException)
            {
                skipped++; // PCRE-only construct .NET rejects; skip so the rest of the dataset still parses
                continue;
            }
            result.Add(project(new CompiledEntry(regex, e)));
        }
        return result.ToArray();
    }

    private static string ReadEmbeddedRegexes()
    {
        var asm = typeof(UapUserAgentParser).Assembly;
        var name = asm.GetManifestResourceNames().First(n => n.EndsWith("regexes.yaml", StringComparison.Ordinal));
        using var stream = asm.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private readonly record struct CompiledEntry(Regex Regex, RegexesYaml.Entry Source)
    {
        public string? Get(string key) => key == "regex" ? Source.Get("regex") : Source.Get(key);
    }

    private sealed record UaParser(Regex Regex, string? Family, string? V1, string? V2, string? V3);
    private sealed record OsParser(Regex Regex, string? Os, string? V1, string? V2, string? V3, string? V4);
    private sealed record DeviceParser(Regex Regex, string? Device, string? Brand, string? Model);
}
