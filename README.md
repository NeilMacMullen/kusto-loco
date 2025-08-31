


If you are using this project please give it a :star: to show your appreciation - thanks!

Feel free to drop over the [Kusto-Loco](https://discord.com/channels/1409404903260164138/1409404903851294816) discord server if you have questions or feedback.

# Kusto-Loco

Kusto-Loco is a set of libraries and applications based around the [Kusto Query Language (KQL)](https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/).  KQL is normally used against data held in [Azure Data Explorer](https://learn.microsoft.com/en-us/azure/data-explorer/) but Kusto-Loco allows you to query in-memory data held in your own applications.


For a super-quick introduction to KQL see [this wiki page](https://github.com/NeilMacMullen/kusto-loco/wiki/Basic-introduction-to-KQL) but to give you a flavour here's a simple query that calculates the average rating of all PCs in a product table and renders the results as a chart.

```kql
products 
| where Category=='PC' 
| join reviews on ProductId 
| summarize Rating=avg(Score) by ProductId,ProductName 
| order by Rating
| project ProductName, Rating
| render columnchart
```


Kusto-Loco makes it easy to load data from CSV, JSON or Parquet files or from sets of POCOs held in memory. Query results can be serialised back to files or objects or rendered to HTML charts using the [Vega-Lite](https://vega.github.io/vega-lite/examples/) charting library.

Loading data, running a query and rendering the results to a chart can be done in a few lines of code after referencing the appropriate [Nuget Packages](https://github.com/NeilMacMullen/kusto-loco/wiki/Applications-and-Nuget-Packages).

```csharp
var context = new KustoQueryContext()
                  .CopyDataIntoTable("products", productRows); // load data from a set of POCOs
var result = await context.RunQuery("products | summarize count() by Category | render piechart");

var datatable = result.ToDataTable(); // create a datatable to dump into a datagrid
webview.NavigateToString(KustoResultRenderer.RenderToHtml(result)); //render chart
```

If you just want to get started playing around with KQL on your own file-based data you can use the supplied [LokqlDX](https://github.com/NeilMacMullen/kusto-loco/wiki/LokqlDX) cross-platform application. 
<img width="1000" height="658" alt="image" src="https://github.com/user-attachments/assets/64025645-bbb7-4ed8-9dc3-3393fcb3d4c0" />



Kusto-Loco even comes with a Powershell module that allows you use KQL queries in a  Powershell pipeline.

![image](https://github.com/NeilMacMullen/kusto-loco/assets/9131337/2522d3f0-9b57-4009-a270-8f6fc13d91a1)

## Quick Starts

- [For application developers who want to consume the libraries](https://github.com/NeilMacMullen/kusto-loco/wiki/Using-the-query-engine)
- [LokqlDX - a simple data explorer for local files](https://github.com/NeilMacMullen/kusto-loco/wiki/LokqlDX)
- [Powershell integration](https://github.com/NeilMacMullen/kusto-loco/wiki/Powershell-Integration)
- [ChatGpt Copilot](https://github.com/NeilMacMullen/kusto-loco/wiki/LokqlDX#chatgpt-copilot)



## Status

The project is still in active development and APIs may change.  However, the core engine is stable and and is actively used in a production environment.  Some KQL operators and functions are not yet implemented. 

## Credits and Contributors

KustoLoco is a fork of the [BabyKusto](https://github.com/davidnx/baby-kusto-csharp) engine created by [DavidNx](https://github.com/davidnx),[Vicky Li](https://github.com/VickyLi2021) and [David Nissimoff](https://github.com/davidni) who appear to have developed the core engine as part of a Microsoft Hackathon.  

Since then the engine has been heavily extended and optimised by [NeilMacMullen](https://github.com/NeilMacMullen) with additional contributions from [Vartika Gupta](https://github.com/vartika-jain-gupta) and [Kosta Demoore](https://github.com/konvolution). 

The project leans heavily on a number of open source libraries including:
- [Benchmark.Net](https://github.com/dotnet/BenchmarkDotNet) used for performance testing
- [CommandLineParser](https://github.com/commandlineparser/commandline) used for command parsing within LokqlDX
- [CsvHelper](https://joshclose.github.io/CsvHelper/) used for CSV serialisation
- [Fashenstein](https://github.com/DanHarltey/Fastenshtein) use for fuzzy string matching
- [FluentAssertions](https://fluentassertions.com/) used for testing
- [geohash-dotnet](https://github.com/postlagerkarte/geohash-dotnet) for geohash encoding
- [NotNullStrings](https://github.com/NeilMacMullen/NotNullStrings) for basic string extensions
- [Parquet.Net](https://github.com/aloneguid/parquet-dotnet) for Parquet serialisation
- [Spectre.Console](https://github.com/spectreconsole/spectre.console) for nice table rendering
- [T-Digest.net](https://github.com/ASolomatin/T-Digest.NET) is used internally for some aggregation functions
- [VegaGenerator](https://github.com/NeilMacMullen/VegaGenerator) is used to simplify rendering of charts

## Contributing

Contributors are always welcome, even if it's 'just' to improve the documentaion ! If you'd like to help with the project please see the [Contributing](https://github.com/NeilMacMullen/kusto-loco/wiki/Contributing) page.
