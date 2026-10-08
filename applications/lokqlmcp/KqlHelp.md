# KustoLoco KQL Help

This server runs queries with KustoLoco, an in-process KQL engine. It supports most of the core Kusto Query Language, but it differs from Azure Data Explorer (ADX) in several ways. Use this guide to avoid common mistakes.

## General
- The only data available is in the tables that ListTables returns. There are no cluster(), database() or external_table() references.
- Table names are case-sensitive. If a name contains special characters, quote it like this: `['my-table']`.
- If a query fails, read the error message, simplify the query and try again. Build complex queries up step by step.
- Use `take` or `limit` while exploring. Use the maxRows parameter of RunQuery to control how many rows come back.

## Unsupported or limited features
- Management/control commands (any command starting with a dot, such as `.show`) are not supported.
- Cross-cluster and cross-database queries are not supported.
- Some operators are missing or only partly implemented. Examples include `evaluate` plugins (most are unavailable), `make-series` (limited), `mv-apply`, `partition`, `scan`, `fork`, `facet`, `find`, `search` and `externaldata`. Prefer `summarize`, `extend`, `project`, `where`, `join`, `union`, `mv-expand`, `top`, `sort`, `distinct` and `count`.
- Some less common scalar and aggregate functions may be missing. If a function is reported as unknown, rewrite the expression with simpler primitives.
- Time-series functions (`series_*`) and machine-learning plugins (`basket`, `autocluster`, `diffpatterns`) may not be available.
- Some `join` kinds and hints may not be supported. Inner, leftouter and similar common kinds work.
- `let` statements and user-defined functions defined inline in the query work. Stored functions do not exist.

## Render
- If a query ends with `render <charttype>` (for example `columnchart`, `barchart`, `linechart`, `piechart`, `scatterchart`, `areachart` or `timechart`), the result comes back as a PNG image.
- `render scatterchart with (kind=map)` cannot be drawn as an image. The tabular data is returned instead.
- Use `with (title="...")` to set the chart title.
- Keep the number of series and points reasonable. Aggregate with `summarize ... by bin(...)` before rendering.

## Types
- Types follow standard KQL: bool, int, long, real, decimal, string, datetime, timespan, guid and dynamic.
- Parquet columns are mapped to the closest KQL type. Use DescribeTable to see the exact types.
- Use explicit conversions such as `todatetime()`, `tolong()` and `tostring()` when comparing values of different types.

## Additional functions
KustoLoco provides these functions that ADX does not have:
- `levenshtein(a, b)`: Levenshtein edit distance between two strings.
- `string_similarity(a, b)`: a value from 0 to 1 that shows how similar two strings are.
- `datetime_to_iso(dt)`: formats a datetime as an ISO 8601 string.
- `trimws(s)`: removes whitespace from the start and end of a string.

## Tips
- Start with `TableName | take 5` or DescribeTable to learn the shape of the data.
- Use `summarize count() by Column` to explore the cardinality of a column.
- Use `project` to return only the columns you need. This keeps results small.
