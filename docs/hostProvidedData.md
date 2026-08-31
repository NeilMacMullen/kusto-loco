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

`externaldata` works **out of the box**: with no host resolver registered the engine uses its built-in
`HttpExternalDataResolver`, so the operator behaves as it does in ADX rather than being inert. Its
defaults are deliberately conservative, because a URI inside a query directs the process to make a
request: **HTTPS only**, the resolved address must be **public** (loopback, link-local — including the
`169.254.169.254` instance-metadata endpoint — unique-local and RFC1918 ranges are refused), redirects
are **re-validated at every hop**, and both a request timeout and a streamed byte cap bound a single
fetch. `gzip` is decompressed and a UTF-8 BOM is stripped.

Register your own resolver to **tighten** that policy (an allow-list of hosts, smaller bounds),
**widen** it (other schemes, authenticated storage), or serve fixtures offline in tests:

```csharp
context.SetExternalDataResolver(new HttpExternalDataResolver(allowedHosts: ["feeds.contoso.com"]));
```

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

### How close this is to ADX

Faithful: the `externaldata (schema) [uris] with(...)` shape, the delimited formats, RFC4180 quoting, unioning
multiple URIs, typing each cell per the declared schema, `ignoreFirstRecord` (dropping the header of **every** URI,
not just the first), and failing the query when a fetch fails.

Deliberately different:

- **No authenticated storage.** ADX reads blob/ADLS/S3 with connection strings and SAS tokens; the built-in resolver
  fetches only public HTTPS. Register your own `IExternalDataResolver` to add credentials.
- **Non-public addresses are refused.** ADX is a managed service; an engine embedded in someone else's process must
  not become an SSRF vector, so loopback, link-local, unique-local and RFC1918 targets are blocked.
- **An unsupported `with(...)` property fails the query** rather than being ignored, because silently dropping a
  property returns a different result than the author asked for with nothing to indicate it.

### Supported formats, and a note on JSON

Only the **delimited** family is supported today. ADX additionally accepts `json`, `multijson`,
`parquet`, `avro` and others.

Delimited formats fit this contract naturally because cells are **positional** — the *n*th cell is
the *n*th declared column. JSON is not positional: its properties map to columns **by name**, and
the resolver is given the format but not the declared column names, so it cannot do that mapping.
Supporting JSON therefore needs a deliberate decision rather than an incremental patch — either the
contract grows to carry the declared column names, or the engine takes raw content and parses it
itself (which would move format handling out of the host, away from the `LoadTablesAsync`
convention this seam follows).

That trade-off is left open on purpose rather than settled by whichever option was easiest to add.
