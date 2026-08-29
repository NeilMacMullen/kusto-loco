# Host-provided data

Some KQL functions and operators are backed by data the engine does not ship: an IP-geolocation
database, a user-agent dataset, or a list fetched from external storage. Bundling those would force
every consumer to take a dependency (and a licence) they may not want, so the engine defines the
capability and the **host supplies the data**.

This follows the same shape as `IKustoQueryContextTableLoader`: the host provides the data and owns
the policy — where it comes from, what is allowed, caching, bounds — and the engine provides the
query surface and helpers.

Providers are registered per `KustoQueryContext`:

```csharp
var context = new KustoQueryContext();
context.AddProvider<IGeoIpProvider>(myGeoProvider);
```

## `geo_info_from_ip_address` — `IGeoIpProvider`

```csharp
public interface IGeoIpProvider
{
    GeoIpInfo? Lookup(IPAddress address);
}

public sealed record GeoIpInfo(
    string? Country = null, string? State = null, string? City = null,
    double? Latitude = null, double? Longitude = null);
```

Returns the ADX shape `{country, state, city, latitude, longitude}`. Return `null` for an address you
cannot resolve — the function then yields `null`, exactly as it does in ADX for an unknown address.
With no provider registered the function still **binds** (a query using it is never rejected) and
resolves to `null`, so a geo predicate is simply inert rather than an error.

A ready-made provider backed by a DB-IP "IP to City Lite" export is available in the optional
`KustoLoco.Geo` package.

## `parse_user_agent` — `IUserAgentParser`

```csharp
public interface IUserAgentParser
{
    UserAgentInfo Parse(string userAgent);
}
```

`UserAgentInfo` carries `Browser` and `OperatingSystem` (`Family`, `Major`, `Minor`, `Patch`,
`PatchMinor`) and `Device` (`Family`, `Brand`, `Model`). As with geo, absent provider ⇒ `null`
result, never a bind failure.

A parser backed by the canonical `ua-parser/uap-core` dataset is available in the optional
`KustoLoco.UserAgent` package.

## `externaldata` — `IExternalDataResolver`

```csharp
public interface IExternalDataResolver
{
    IReadOnlyList<IReadOnlyList<string>> ResolveRows(string uri, string format);
}

context.SetExternalDataResolver(myResolver);
```

The engine performs **no network or file access of its own** and registers **no default resolver**,
so `externaldata` reports an error until a host opts in. That is deliberate: the host is the only
place that can decide which URI schemes are acceptable, what size and time bounds apply, and how to
authenticate.

Two rules matter when implementing one:

- **Throw on failure.** Returning an empty set would turn an unreachable list into a silent
  no-match — the worst outcome for a query whose purpose is matching against that list.
- **Return rows of cells, not typed values.** The engine types each cell per the schema the query
  declared, the same path a `datatable` literal takes.

The engine publishes `DelimitedTextParser` so you do not have to reimplement the declared format:

```csharp
public IReadOnlyList<IReadOnlyList<string>> ResolveRows(string uri, string format)
{
    var text = Fetch(uri);                    // your transport, your policy
    return DelimitedTextParser.Parse(text, DelimitedTextParser.DelimiterFor(format));
}
```

`DelimiterFor` maps the KQL delimited formats — `csv`, `tsv`/`tsve`, `scsv`, `sohsv`, `psv` — and
`Parse` handles RFC4180 quoting.
