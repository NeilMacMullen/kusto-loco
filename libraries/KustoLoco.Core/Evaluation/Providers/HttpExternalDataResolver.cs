//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

namespace KustoLoco.Core;

/// <summary>
/// The engine's default <see cref="IExternalDataResolver"/>: fetches an <c>externaldata</c> URI over HTTPS and
/// splits it with <see cref="DelimitedTextParser"/>, so the operator resolves out of the box the way it does in
/// ADX rather than being inert until a host writes its own fetcher.
/// </summary>
/// <remarks>
/// <para><b>Safe by default.</b> A KQL query is data, and an <c>externaldata</c> URI inside one directs the process
/// to make a request — so the defaults are chosen to make that safe rather than to be maximally permissive:
/// HTTPS only; the resolved address must be public (loopback, link-local — including the 169.254.169.254 cloud
/// instance-metadata endpoint — unique-local, and private RFC1918 ranges are refused); redirects are followed but
/// re-validated at every hop, so a public URL cannot bounce to an internal one; a request timeout and a streamed
/// byte cap bound how long and how much a single query can pull; <c>gzip</c> content is decompressed.</para>
/// <para><b>Tightening or replacing it.</b> Construct one with an explicit allow-list of hosts (or different
/// bounds) and register it with <see cref="KustoQueryContext.SetExternalDataResolver"/>; a host with different
/// needs — authenticated storage, a cache, an offline fixture — registers its own implementation instead. Pass
/// an <see cref="HttpMessageHandler"/> to route requests through existing infrastructure or a test transport.</para>
/// <para>Failures throw: a broken feed must fail the query loudly, since returning no rows would silently turn a
/// fetch error into a no-match.</para>
/// </remarks>
public sealed class HttpExternalDataResolver : IExternalDataResolver, IDisposable
{
    /// <summary>Default ceiling on a single fetch (16 MiB), bounding how much one query can pull into memory.</summary>
    public const int DefaultMaxBytes = 16 * 1024 * 1024;

    /// <summary>Default per-request timeout.</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    private const int MaxRedirects = 5;

    private readonly HttpClient _http;
    private readonly bool _ownsHttp;
    private readonly IReadOnlyCollection<string>? _allowedHosts;
    private readonly int _maxBytes;

    /// <summary>
    /// Create a resolver with the default bounds. <paramref name="allowedHosts"/> restricts fetches to those hosts
    /// (case-insensitive, exact match); null allows any public host.
    /// </summary>
    public HttpExternalDataResolver(
        IEnumerable<string>? allowedHosts = null,
        TimeSpan? timeout = null,
        int maxBytes = DefaultMaxBytes,
        HttpMessageHandler? handler = null)
    {
        if (maxBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxBytes), maxBytes, "maxBytes must be positive.");
        _maxBytes = maxBytes;
        _allowedHosts = allowedHosts?.Select(h => h.Trim()).Where(h => h.Length > 0).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Redirects are handled here (each hop re-validated), never silently by the handler.
        _ownsHttp = handler is null;
        var inner = handler ?? new HttpClientHandler { AllowAutoRedirect = false };
        if (inner is HttpClientHandler h) h.AllowAutoRedirect = false;
        _http = new HttpClient(inner, disposeHandler: _ownsHttp) { Timeout = timeout ?? DefaultTimeout };
    }

    /// <inheritdoc />
    public IReadOnlyList<IReadOnlyList<string>> ResolveRows(string uri, string format)
    {
        var text = Fetch(uri);
        return DelimitedTextParser.Parse(text, DelimitedTextParser.DelimiterFor(format));
    }

    private string Fetch(string uri)
    {
        var target = Validate(uri);
        for (var hop = 0; ; hop++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, target);
            var response = _http.Send(request, HttpCompletionOption.ResponseHeadersRead);
            try
            {
                if (IsRedirect(response.StatusCode))
                {
                    if (hop >= MaxRedirects)
                        throw new InvalidOperationException($"externaldata '{uri}' exceeded {MaxRedirects} redirects.");
                    var location = response.Headers.Location
                        ?? throw new InvalidOperationException($"externaldata '{uri}' returned a redirect with no Location header.");
                    // Re-validate EVERY hop: a public URL must not be able to bounce the fetch to an internal address.
                    target = Validate(new Uri(target, location).ToString());
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException($"externaldata '{uri}' returned HTTP {(int)response.StatusCode}.");

                return ReadBounded(response, uri);
            }
            finally
            {
                response.Dispose();
            }
        }
    }

    private Uri Validate(string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
            throw new InvalidOperationException($"externaldata '{uri}' is not an absolute URI.");
        if (!string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"externaldata '{uri}' must use https (got '{parsed.Scheme}'). Register a custom IExternalDataResolver to allow other schemes.");
        if (_allowedHosts is { Count: > 0 } && !_allowedHosts.Contains(parsed.Host))
            throw new InvalidOperationException($"externaldata host '{parsed.Host}' is not in the configured allow-list.");

        // Resolve and check EVERY address the name maps to — a name that resolves to any non-public address is refused,
        // so a public-looking hostname cannot be pointed at internal infrastructure.
        var addresses = ResolveAddresses(parsed);
        foreach (var address in addresses)
            if (!IsPublic(address))
                throw new InvalidOperationException(
                    $"externaldata '{uri}' resolves to the non-public address {address}; only public addresses are fetched.");
        return parsed;
    }

    private static IReadOnlyList<IPAddress> ResolveAddresses(Uri uri)
    {
        if (IPAddress.TryParse(uri.Host, out var literal)) return new[] { literal };
        try
        {
            return Dns.GetHostAddresses(uri.Host);
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException($"externaldata host '{uri.Host}' could not be resolved: {ex.Message}", ex);
        }
    }

    // Public = not loopback/link-local/unique-local/private/multicast/unspecified. Link-local covers the cloud
    // instance-metadata endpoint (169.254.169.254), the classic SSRF credential target.
    private static bool IsPublic(IPAddress address)
    {
        if (IPAddress.IsLoopback(address)) return false;
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var b = address.GetAddressBytes();
            if (b[0] == 0 || b[0] == 10 || b[0] == 127) return false;                 // unspecified, RFC1918 /8, loopback
            if (b[0] == 169 && b[1] == 254) return false;                             // link-local (incl. IMDS)
            if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return false;                // RFC1918 /12
            if (b[0] == 192 && b[1] == 168) return false;                             // RFC1918 /16
            if (b[0] == 100 && b[1] >= 64 && b[1] <= 127) return false;               // CGNAT
            if (b[0] >= 224) return false;                                            // multicast + reserved
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast) return false;
            if (address.Equals(IPAddress.IPv6Any) || address.Equals(IPAddress.IPv6Loopback)) return false;
            var b = address.GetAddressBytes();
            if ((b[0] & 0xFE) == 0xFC) return false;                                  // unique-local fc00::/7
            return true;
        }

        return false;
    }

    private string ReadBounded(HttpResponseMessage response, string uri)
    {
        // Declared length is a cheap early reject; the streamed cap below is what actually bounds an
        // undeclared or lying response.
        if (response.Content.Headers.ContentLength is { } declared && declared > _maxBytes)
            throw new InvalidOperationException(
                $"externaldata '{uri}' is {declared} bytes, over the {_maxBytes}-byte limit.");

        using var raw = response.Content.ReadAsStream();
        using var stream = IsGzip(response) ? new GZipStream(raw, CompressionMode.Decompress) : raw;

        var buffer = new byte[81920];
        using var memory = new MemoryStream();
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (memory.Length + read > _maxBytes)
                throw new InvalidOperationException(
                    $"externaldata '{uri}' exceeded the {_maxBytes}-byte limit.");
            memory.Write(buffer, 0, read);
        }

        // Decode as UTF-8, skipping a byte-order mark if the feed emits one — published CSV exports commonly do,
        // and a retained BOM would corrupt the first cell of the first row.
        var bytes = memory.ToArray();
        var offset = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
        return Encoding.UTF8.GetString(bytes, offset, bytes.Length - offset);
    }

    private static bool IsGzip(HttpResponseMessage response) =>
        response.Content.Headers.ContentEncoding.Any(e => string.Equals(e, "gzip", StringComparison.OrdinalIgnoreCase))
        || (response.RequestMessage?.RequestUri?.AbsolutePath.EndsWith(".gz", StringComparison.OrdinalIgnoreCase) ?? false);

    private static bool IsRedirect(HttpStatusCode code) =>
        code is HttpStatusCode.Moved or HttpStatusCode.Found or HttpStatusCode.SeeOther
             or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsHttp) _http.Dispose();
    }
}
