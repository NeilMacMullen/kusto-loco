//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using KustoLoco.Core;

namespace KustoLoco.Geo;

/// <summary>
/// The DB-IP Lite CSV layout a <see cref="DbIpGeoProvider"/> reads. DB-IP publishes several free "Lite" feeds that
/// share the <c>ip_start,ip_end,…</c> prefix but differ in the columns that follow; a compact "country + centroid"
/// derivation (country-lite joined to per-country coordinates) is also common where city-level precision is not
/// needed. The three are unambiguous by column count, so <see cref="DbIpLayout.Auto"/> is the usual choice.
/// </summary>
public enum DbIpLayout
{
    /// <summary>Detect the layout from the first data row's column count (8+ ⇒ City, 5–7 ⇒ Country+centroid, ≤4 ⇒ Country).</summary>
    Auto,

    /// <summary>DB-IP <b>Country</b> Lite: <c>ip_start,ip_end,country</c>. Country only — coordinates resolve to null.</summary>
    CountryLite,

    /// <summary>Compact <b>country + centroid</b>: <c>ip_start,ip_end,country,latitude,longitude</c>. Country-level
    /// geo with coordinates (per-country centroids), adequate for country-scale geo-distance without a City feed.</summary>
    CountryCentroid,

    /// <summary>DB-IP <b>City</b> Lite: <c>ip_start,ip_end,continent,country,stateprov,city,latitude,longitude</c>. Full fidelity.</summary>
    CityLite,
}

/// <summary>
/// An <see cref="IGeoIpProvider"/> backed by a DB-IP "Lite" CSV export (CC-BY-4.0 — attribution required, see the
/// NOTICE file). KustoLoco's <c>geo_info_from_ip_address</c> function is native in the core engine; this optional
/// companion package supplies the lookup data, mirroring real ADX (whose same function is itself built on a
/// downloadable geo database). All three DB-IP Lite layouts are read (see <see cref="DbIpLayout"/>) — City Lite
/// (city + coordinates), the compact Country+centroid derivation (country + coordinates), and Country Lite
/// (country only) — auto-detected by column count. Both IPv4 and IPv6 are supported.
///
/// The dataset is parsed once into sorted range tables and resolved by binary search, so lookups are O(log n)
/// and the provider is immutable and safe to share across concurrent queries. Build it at host startup and
/// register it: <c>context.AddProvider&lt;IGeoIpProvider&gt;(DbIpGeoProvider.FromFile(path))</c>. When an address
/// is unparseable or not covered, <see cref="Lookup"/> returns null, so the function yields null (inert).
/// </summary>
public sealed class DbIpGeoProvider : IGeoIpProvider
{
    private readonly RangeTable<uint> _v4;
    private readonly RangeTable<UInt128> _v6;

    private DbIpGeoProvider(RangeTable<uint> v4, RangeTable<UInt128> v6)
    {
        _v4 = v4;
        _v6 = v6;
    }

    /// <summary>Total number of parsed ranges. 0 for a present-but-unparseable dataset (wrong delimiter/format).</summary>
    public int RangeCount => _v4.Count + _v6.Count;

    public GeoIpInfo? Lookup(IPAddress address)
    {
        if (address is null)
            return null;
        return address.AddressFamily switch
        {
            AddressFamily.InterNetwork => _v4.Find(ToUInt32(address)),
            AddressFamily.InterNetworkV6 => _v6.Find(ToUInt128(address)),
            _ => null
        };
    }

    /// <summary>Build from a DB-IP Lite CSV file, auto-detecting the layout. A <c>.gz</c> extension is decompressed transparently.</summary>
    public static DbIpGeoProvider FromFile(string path) => FromFile(path, DbIpLayout.Auto);

    /// <summary>Build from a DB-IP Lite CSV file with an explicit <paramref name="layout"/>. A <c>.gz</c> extension is decompressed transparently.</summary>
    public static DbIpGeoProvider FromFile(string path, DbIpLayout layout) => FromLines(ReadLines(path), layout);

    /// <summary>
    /// Build from CSV lines in any DB-IP Lite layout (see <see cref="DbIpLayout"/>). With
    /// <see cref="DbIpLayout.Auto"/> (the default) the layout is detected from the first data row's column count and
    /// held for the whole file — a single dataset is one layout. Blank, comment ('#') and header lines (whose first
    /// field is not an IP) are skipped. country is a 2-letter ISO code, surfaced as the English country name (ADX
    /// shape) via <see cref="RegionInfo"/>, falling back to the raw code; State/City/coordinates are null for layouts
    /// that do not carry them.
    /// </summary>
    public static DbIpGeoProvider FromLines(IEnumerable<string> lines, DbIpLayout layout = DbIpLayout.Auto)
    {
        var v4 = new List<(uint Start, uint End, GeoIpInfo Info)>();
        var v6 = new List<(UInt128 Start, UInt128 End, GeoIpInfo Info)>();
        LayoutMap? map = layout == DbIpLayout.Auto ? null : LayoutMap.For(layout);

        foreach (var raw in lines)
        {
            if (string.IsNullOrWhiteSpace(raw) || raw[0] == '#')
                continue;
            var f = SplitCsv(raw);
            if (f.Length < 3)
                continue;
            if (!IPAddress.TryParse(f[0], out var start) || !IPAddress.TryParse(f[1], out var end))
                continue; // header row or malformed line
            if (start.AddressFamily != end.AddressFamily)
                continue;

            // First data row (first row whose leading two fields are IPs) fixes the layout for the whole file; the
            // column count of a real DB-IP data row disambiguates the three Lite variants unambiguously.
            map ??= LayoutMap.For(LayoutMap.Detect(f.Length));
            var m = map.Value;

            var info = new GeoIpInfo(
                Country: CountryName(Field(f, m.Country)),
                State: m.State >= 0 ? Field(f, m.State) : null,
                City: m.City >= 0 ? Field(f, m.City) : null,
                Latitude: m.Lat >= 0 ? ParseCoordinate(Field(f, m.Lat)) : null,
                Longitude: m.Lon >= 0 ? ParseCoordinate(Field(f, m.Lon)) : null);

            if (start.AddressFamily == AddressFamily.InterNetwork)
            {
                var s = ToUInt32(start);
                var e = ToUInt32(end);
                if (s > e) (s, e) = (e, s);
                v4.Add((s, e, info));
            }
            else if (start.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var s = ToUInt128(start);
                var e = ToUInt128(end);
                if (s > e) (s, e) = (e, s);
                v6.Add((s, e, info));
            }
        }

        return new DbIpGeoProvider(RangeTable<uint>.Build(v4), RangeTable<UInt128>.Build(v6));
    }

    private static string? Field(string[] fields, int index) =>
        index >= 0 && index < fields.Length && fields[index].Length > 0 ? fields[index] : null;

    // Column indices of the fields we surface, per layout. A negative index means the layout does not carry that
    // field (so it resolves to null). This is the single place the three DB-IP Lite column orders are encoded.
    private readonly record struct LayoutMap(int Country, int State, int City, int Lat, int Lon)
    {
        public static LayoutMap For(DbIpLayout layout) => layout switch
        {
            DbIpLayout.CityLite => new LayoutMap(Country: 3, State: 4, City: 5, Lat: 6, Lon: 7),
            DbIpLayout.CountryCentroid => new LayoutMap(Country: 2, State: -1, City: -1, Lat: 3, Lon: 4),
            DbIpLayout.CountryLite => new LayoutMap(Country: 2, State: -1, City: -1, Lat: -1, Lon: -1),
            _ => throw new ArgumentOutOfRangeException(nameof(layout), layout, "Auto must be resolved before mapping."),
        };

        // 8+ columns is City Lite (…,city,latitude,longitude); 5–7 is the compact country+centroid derivation
        // (country,latitude,longitude); 3–4 is Country Lite (country only). A single dataset is one layout.
        public static DbIpLayout Detect(int fieldCount) => fieldCount switch
        {
            >= 8 => DbIpLayout.CityLite,
            >= 5 => DbIpLayout.CountryCentroid,
            _ => DbIpLayout.CountryLite,
        };
    }

    private static double? ParseCoordinate(string? s) =>
        s is not null && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;

    // DB-IP stores the country as a 2-letter ISO 3166 code; ADX returns the country *name*. RegionInfo maps the
    // code to its English name with no data table of our own, falling back to the raw value for non-ISO codes.
    private static string? CountryName(string? code)
    {
        if (string.IsNullOrEmpty(code))
            return null;
        if (code.Length != 2)
            return code;
        try
        {
            return new RegionInfo(code).EnglishName;
        }
        catch (ArgumentException)
        {
            return code;
        }
    }

    private static uint ToUInt32(IPAddress address)
    {
        var b = address.GetAddressBytes();
        return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
    }

    private static UInt128 ToUInt128(IPAddress address)
    {
        var b = address.GetAddressBytes(); // 16 bytes, network (big-endian) order
        UInt128 value = 0;
        foreach (var octet in b)
            value = (value << 8) | octet;
        return value;
    }

    private static IEnumerable<string> ReadLines(string path)
    {
        if (!path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var line in File.ReadLines(path))
                yield return line;
            yield break;
        }

        using var fs = File.OpenRead(path);
        using var gz = new GZipStream(fs, CompressionMode.Decompress);
        using var sr = new StreamReader(gz);
        for (var line = sr.ReadLine(); line is not null; line = sr.ReadLine())
            yield return line;
    }

    // Minimal RFC4180 field splitter: honours double-quoted fields (so a city name containing a comma is intact)
    // and doubled quotes as an escaped quote. Sufficient for the flat DB-IP City Lite rows.
    private static string[] SplitCsv(string line)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQuotes = false;
                }
                else sb.Append(c);
            }
            else if (c == '"') inQuotes = true;
            else if (c == ',') { fields.Add(sb.ToString().Trim()); sb.Clear(); }
            else sb.Append(c);
        }
        fields.Add(sb.ToString().Trim());
        return fields.ToArray();
    }

    // An immutable, sorted set of key ranges resolved by binary search. The backward walk from the last range
    // whose start <= key resolves the most-specific enclosing range, so a nested range still resolves correctly;
    // for the flat, non-overlapping DB-IP data it is a single comparison.
    private sealed class RangeTable<TKey> where TKey : struct, IComparable<TKey>
    {
        private readonly TKey[] _starts;
        private readonly TKey[] _ends;
        private readonly GeoIpInfo[] _info;

        private RangeTable(TKey[] starts, TKey[] ends, GeoIpInfo[] info)
        {
            _starts = starts;
            _ends = ends;
            _info = info;
        }

        public int Count => _starts.Length;

        public static RangeTable<TKey> Build(List<(TKey Start, TKey End, GeoIpInfo Info)> rows)
        {
            rows.Sort((a, b) => a.Start.CompareTo(b.Start));
            return new RangeTable<TKey>(
                rows.Select(r => r.Start).ToArray(),
                rows.Select(r => r.End).ToArray(),
                rows.Select(r => r.Info).ToArray());
        }

        public GeoIpInfo? Find(TKey key)
        {
            int lo = 0, hi = _starts.Length - 1, found = -1;
            while (lo <= hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                if (_starts[mid].CompareTo(key) <= 0) { found = mid; lo = mid + 1; }
                else hi = mid - 1;
            }
            for (var i = found; i >= 0; i--)
                if (key.CompareTo(_ends[i]) <= 0)
                    return _info[i];
            return null;
        }
    }
}
