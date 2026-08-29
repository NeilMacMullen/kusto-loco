# KustoLoco.Geo

An optional geo-IP provider for KustoLoco's `geo_info_from_ip_address()` function.

KustoLoco's core engine implements `geo_info_from_ip_address()` natively but ships **no**
geo database — geolocation datasets are large and their licences vary, and not every
consumer wants the dependency. Instead, the function reads from an `IGeoIpProvider` that
the host registers. This package is one such provider, backed by a
[DB-IP "Lite"](https://db-ip.com/db/lite.php) CSV export — City Lite, Country Lite, or a
compact country+centroid derivation, all read by the same provider.

This mirrors real Azure Data Explorer, whose `geo_info_from_ip_address()` is likewise
built on a downloadable geo database (GeoLite2).

## Usage

1. Download the **DB-IP IP to City Lite** database in **CSV** form (`.csv` or `.csv.gz`).
2. Build the provider once at startup and register it on your query context:

```csharp
using KustoLoco.Core;
using KustoLoco.Geo;

var context = new KustoQueryContext();
context.AddProvider<IGeoIpProvider>(DbIpGeoProvider.FromFile("dbip-city-lite.csv.gz"));

// geo_info_from_ip_address now resolves against the dataset:
var result = await context.RunQuery(
    "print geo = geo_info_from_ip_address('8.8.8.8')");
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
