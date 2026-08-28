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

// externaldata resolver. The engine never performs network access itself: it hands the declared schema, the requested
// URIs and the format to a host-registered resolver, which fetches and returns the rows. No resolver registered means
// externaldata is unavailable (fail-closed) - the host must opt in, and is where URL-allowlisting / SSRF protection /
// authentication live. Returning the rows column-major matches how the engine builds a table.
public interface IExternalDataResolver
{
    // Resolve the data for one externaldata expression. 'columnTypes' are the declared column .NET types (in schema
    // order); the implementation returns one object?[] per column, all of equal length. Return null to yield an empty
    // table. Throw to fail the query (e.g. a disallowed URI).
    IReadOnlyList<object?[]>? Resolve(ExternalDataRequest request);
}

public sealed record ExternalDataRequest(
    IReadOnlyList<string> Uris,
    string Format,
    IReadOnlyList<string> ColumnNames,
    IReadOnlyList<System.Type> ColumnTypes);

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
