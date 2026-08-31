# KustoLoco.Geo

An optional geo-IP provider for KustoLoco's `geo_info_from_ip_address()` function.

KustoLoco's core engine implements `geo_info_from_ip_address()` natively but carries **no**
geo dependency — geolocation datasets are large and their licences vary, so the function
reads from an `IGeoIpProvider` that the host registers. This package is that provider, and
it **embeds a default database** ([DB-IP IP-to-Country Lite](https://db-ip.com/db/lite.php)
joined with Google canonical country centroids, both CC-BY-4.0) so geo lookups work with
**one line and no data file**:

```csharp
context.AddProvider<IGeoIpProvider>(DbIpGeoProvider.Default);
```

Point it at your own [DB-IP "Lite"](https://db-ip.com/db/lite.php) export — City Lite,
Country Lite, or a compact country+centroid derivation — when you want city-level
precision or IPv6 coverage.

This mirrors real Azure Data Explorer, whose `geo_info_from_ip_address()` is likewise
built on a downloadable geo database (GeoLite2).

## Usage

Register the provider on your query context — the embedded database means there is nothing
to download and no file to ship:

```csharp
using KustoLoco.Core;
using KustoLoco.Geo;

var context = new KustoQueryContext();
context.AddProvider<IGeoIpProvider>(DbIpGeoProvider.Default);

// geo_info_from_ip_address now resolves:
var result = await context.RunQuery(
    "print geo = geo_info_from_ip_address('8.8.8.8')");
```

### Bringing your own database

For city/state precision or IPv6 coverage, download a **DB-IP Lite** database in **CSV**
form (`.csv` or `.csv.gz`) and build the provider from it instead:

```csharp
context.AddProvider<IGeoIpProvider>(DbIpGeoProvider.FromFile("dbip-city-lite.csv.gz"));
```

The provider is immutable and thread-safe once built, so a single instance can serve
concurrent queries. Both IPv4 and IPv6 addresses are supported. An address that is
unparseable or not covered by the dataset yields a `null` result, exactly as the ADX
function does.

### Output shape

`geo_info_from_ip_address()` returns a dynamic object with the ADX fields:

```json
{ "country": "...", "state": "...", "city": "...", "latitude": 0.0, "longitude": 0.0 }
```

`country` is surfaced as the English country name (the DB-IP `country` ISO code is mapped
via `System.Globalization.RegionInfo`).

## Dataset format

Any of the three DB-IP Lite layouts works; `FromFile`/`FromLines` **auto-detect** by the
first data row's column count (pass a `DbIpLayout` to force one). `country` is a 2-letter
ISO code in every layout, surfaced as the English country name. A single file is one layout.

| Layout | Columns | Coordinates |
|---|---|---|
| **City Lite** (`DbIpLayout.CityLite`) | `ip_start,ip_end,continent,country,stateprov,city,latitude,longitude` | city-level |
| **Country + centroid** (`DbIpLayout.CountryCentroid`) | `ip_start,ip_end,country,latitude,longitude` | per-country centroid |
| **Country Lite** (`DbIpLayout.CountryLite`) | `ip_start,ip_end,country` | none (null) |

City Lite gives full fidelity; the compact country+centroid layout keeps a much smaller
file while still answering country-scale geo-distance; Country Lite is country-only.

## Attribution

The DB-IP Lite databases are licensed **CC-BY-4.0**; distributing an application that uses
one requires attributing DB-IP. See [`NOTICE`](NOTICE).
