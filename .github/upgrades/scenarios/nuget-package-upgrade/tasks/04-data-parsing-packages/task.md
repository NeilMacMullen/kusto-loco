# 04-data-parsing-packages: data-parsing-packages

Update data/parsing packages excluding the two flagged breaking majors: `ClosedXML`, `ExcelDataReader.DataSet`, `ShapeCrawler`, `Magick.NET-Q16-AnyCPU`, `Newtonsoft.Json`, `Snappier`, `Tmds.DBus.Protocol`, `SkiaSharp.HarfBuzz` (if referenced), `geohash-dotnet`, `Microsoft.Azure.Kusto.Data`, `Microsoft.Azure.Kusto.Language`. **Skip** `CsvHelper` (33.1.0→34.0.0, major breaking) and `Parquet.Net` (5.6.0→6.1.0, major breaking) — leave at current pinned versions.

**Done when**: Non-breaking packages updated, CsvHelper and Parquet.Net left unchanged, solution builds, tests pass.
