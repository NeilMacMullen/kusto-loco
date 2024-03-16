# Kusto-Loco

Kusto-Loco makes it easy to use the Kusto Query Language (KQL) to query data held in your own applications or local files.  You can use it as an engine for your own application or just download the prebuilt tools to explore your existing data.

Data can be loaded from CSV, JSON or Parquet files or from sets of POCOs held in memory. Query results can be serialised back to files or objects or rendered to HTML charts.

The engine is extensible to allow you to provide your own custom functions for appropriate for your domain.

The main components are:
  
- Nuget Packages
	- KustoLoco.Core - the core query engine for embedding in your own application
	- KustoLoco.FileFormats - a set of file readers and writers for standard formats
	- KustLoco.Rendering - supports rendering to HTML charts using Vega-Lite
	- KustoLoco.SourceGeneration - used for creating custom functions
- Applications
    - Lokql - a scriptable command-line tool for data exploration and manipulation
	- LokqlDx - a more capable WPF version of Lokql
	- PSKql - brings the power of KQL to Powershell.  Allows querying/manipulation of piped data.


## Quick Start

### Querying in-memory data
```csharp
record Temperature(string City,double Temperature,DateTime Date);
record MaxTempByCity(double  MaxTemp,DateTime Day,string City);

var temperatures = .... // get a set of Temperature records

var query = @"where Date > date(1 jan 2000) 
              | project Day=bin(Date,1d) 
			  | summarize MaxTemp=max(Temperature) by Day,City
			  | order by MaxTemp
			  | take 10
			 ";

var result = KustoQueryContext.QueryRecords(processes, query);

//if you know what type the result is you can turn it into a strongly-typed set
var maxTemperatures = result.ToRecords<MaxTempByCity>();

//if you just want to see the results in a datagrid (for example)
foreach (var col in result.ColumnNames())   dataTable.Columns.Add(col);
foreach (var row in result.EnumerateRows()) dataTable.Rows.Add(row);

//if you want to send the results across the wire without knowing
//their shape..
var dto = new MyDto{ 
    IssuedQuery=Result.Query, 
	Error=Result.Error,
	Data = result.ToSerializableObject() 
	 };


```


## Credits



## Lokql
Lokql is a a simple command-line based data explorer that allows you to load data from files, issue KQL queries, and render the results to charts or tables.

TODO:Example here

## Project Goals

| Goal | Non-Goal|
|------|----------|
|Provide a *useful* implementation of a significant subset of the standard Kusto Query Language and built-in functions. | Provide "bit-exact" results vs ADX |
|Easy import/export of local file-based and in-memory data | Distributed/cluster-based processing |
|"Good enough" performance for single-user interaction | Low-latency, "web-scale" query serving |
| Allow engine extensibility for custom functionality |Fork Kusto Language |
| Basic query optimisation | Complex query planner |

## Changes relative to original BabyKusto
- Much more efficient filtering
- CSV/POCO adaptation layer
- Tabulation of results
- Additional functions and operations

## Known differences to ADX/Kusto
- Regex operations use C# regex format
- GeoHash limited to precision of 12 
- No clustering
- Rendering is translated via vega-lite
- row_number method is non-compliant

TODO 


## Credits


KustoLoco is a fork of the [BabyKusto](https://github.com/davidnx/baby-kusto-csharp) engine created by [DavidNx](https://github.com/davidnx),[Vicky Li](https://github.com/VickyLi2021) and [David Nissimoff](https://github.com/davidni) who appear to have developed the core engine as part of a Microsoft Hackathon.  

Since then the engine has been extended and optimised by the [Sensize](https://sensize.net) team including NeilMacMullen, Vartika Gupta and Kosta Demoore.

## WIP
- Use index-based colums for Join
- Port Sensize extensions
- Port "explore" application
- Parquet serialization

## Other resources 
- wiki (todo) 
- [ADX playpen](https://dataexplorer.azure.com/clusters/help/databases/Samples)
- 

## Help Wanted

To come... primarily filling out the function/operation set

