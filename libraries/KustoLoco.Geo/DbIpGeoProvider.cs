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
/// An <see cref="IGeoIpProvider"/> backed by a DB-IP "IP to City Lite" CSV export (CC-BY-4.0 — attribution
/// required, see the NOTICE file). KustoLoco's <c>geo_info_from_ip_address</c> function is native in the core
/// engine; this optional companion package supplies the lookup data, mirroring real ADX (whose same function is
/// itself built on a downloadable geo database). Both IPv4 and IPv6 are supported.
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

    /// <summary>Build from a DB-IP City Lite CSV file. A <c>.gz</c> extension is decompressed transparently.</summary>
    public static DbIpGeoProvider FromFile(string path) => FromLines(ReadLines(path));

    /// <summary>
    /// Build from CSV lines with the DB-IP City Lite layout:
    /// <c>ip_start,ip_end,continent,country,stateprov,city,latitude,longitude</c>. Blank, comment ('#') and
    /// header lines (whose first field is not an IP) are skipped. country is a 2-letter ISO code, surfaced as the
    /// English country name (ADX shape) via <see cref="RegionInfo"/>, falling back to the raw code.
    /// </summary>
    public static DbIpGeoProvider FromLines(IEnumerable<string> lines)
    {
        var v4 = new List<(uint Start, uint End, GeoIpInfo Info)>();
        var v6 = new List<(UInt128 Start, UInt128 End, GeoIpInfo Info)>();

        foreach (var raw in lines)
        {
            if (string.IsNullOrWhiteSpace(raw) || raw[0] == '#')
                continue;
            var f = SplitCsv(raw);
            if (f.Length < 4)
                continue;
            if (!IPAddress.TryParse(f[0], out var start) || !IPAddress.TryParse(f[1], out var end))
                continue; // header row or malformed line
            if (start.AddressFamily != end.AddressFamily)
                continue;

            var info = new GeoIpInfo(
                Country: CountryName(Field(f, 3)),
                State: Field(f, 4),
                City: Field(f, 5),
                Latitude: ParseCoordinate(Field(f, 6)),
                Longitude: ParseCoordinate(Field(f, 7)));

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
        index < fields.Length && fields[index].Length > 0 ? fields[index] : null;

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
