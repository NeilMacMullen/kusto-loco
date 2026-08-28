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
