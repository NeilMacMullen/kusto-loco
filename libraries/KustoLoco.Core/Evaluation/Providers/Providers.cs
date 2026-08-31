//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;

namespace KustoLoco.Core;

// A per-query registry of host-supplied providers that data-backed scalar functions read at evaluation time. It is
// registered on the engine and threaded through the EvaluationContext. When a function's provider is absent the
// function returns null (inert) rather than failing, so queries that reference it still run.
public interface IProviderRegistry
{
    T? Get<T>() where T : class;
}

public sealed class ProviderRegistry : IProviderRegistry
{
    private readonly Dictionary<Type, object> _providers = new();

    public ProviderRegistry Set<T>(T provider) where T : class
    {
        _providers[typeof(T)] = provider;
        return this;
    }

    public T? Get<T>() where T : class =>
        _providers.TryGetValue(typeof(T), out var p) ? (T)p : null;
}

// geo_info_from_ip_address data provider. The engine ships no geo database (licences vary and not every consumer wants
// the dependency); the host supplies the IP -> geo lookup. Returning null for an address yields a null function result.
public interface IGeoIpProvider
{
    GeoIpInfo? Lookup(IPAddress address);
}

public sealed record GeoIpInfo(
    string? Country = null,
    string? State = null,
    string? City = null,
    double? Latitude = null,
    double? Longitude = null);

/// <summary>
/// Resolves an <c>externaldata</c> URI to delimited rows (already split into string cells). The engine calls this
/// once per URI in an <c>externaldata (schema) [ "uri", ... ] with (format=...)</c> expression, then types each cell
/// per the declared schema and materializes a table — the same path a <c>datatable</c> literal takes.
/// </summary>
/// <remarks>
/// The implementation owns ALL fetch policy: which URI schemes are allowed, size and time bounds, authentication and
/// caching. This mirrors <see cref="IKustoQueryContextTableLoader"/>, where the host likewise supplies the data and
/// owns the policy; the engine supplies <see cref="DelimitedTextParser"/> so the declared format need not be
/// reimplemented. It MUST throw on a fetch or parse failure so the query fails loudly — returning an empty set would
/// silently turn a broken feed into a no-match. When no host resolver is registered the engine falls back to
/// <see cref="HttpExternalDataResolver"/>, so <c>externaldata</c> resolves out of the box (HTTPS, public
/// addresses only, bounded time and size); register your own to tighten, widen or replace that policy.
/// </remarks>
public interface IExternalDataResolver
{
    IReadOnlyList<IReadOnlyList<string>> ResolveRows(string uri, string format);
}

// parse_user_agent data provider. The engine ships no user-agent database; a faithful implementation (e.g. one backed
// by the uap-core dataset) lives in a companion package and is registered by the host.
public interface IUserAgentParser
{
    UserAgentInfo Parse(string userAgent);
}

public sealed record UserAgentInfo(
    UserAgentSoftware Browser,
    UserAgentSoftware OperatingSystem,
    UserAgentDevice Device);

public sealed record UserAgentSoftware(
    string? Family = null,
    string? Major = null,
    string? Minor = null,
    string? Patch = null,
    string? PatchMinor = null);

public sealed record UserAgentDevice(
    string? Family = null,
    string? Brand = null,
    string? Model = null);
