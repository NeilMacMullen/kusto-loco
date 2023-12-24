# !!! UNDER CONSTRUCTION !!!

This fork is currently undergoing changes and is unstable. See [todo-list](docs/planned_work.md)
Please refer back to the original repo for a more stable version

# Kusto-Loco

KustoLoco is a fork of the [BabyKusto](https://github.com/davidnx/baby-kusto-csharp) engine created by [DavidNx](https://github.com/davidnx),[Vicky Li](https://github.com/VickyLi2021) and [David Nissimoff](https://github.com/davidni) which has been [extended](docs/additionalFunctions.md) to improve query performance to provide a richer feature-set.

It provides a simple way to perform complex queries against in-memory tabular data.


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
- Rendering ?

TODO 


## Credits
Credit for original implementation and all heavy-lifting belongs to [DavidNx](https://github.com/davidnx),[Vicky Li](https://github.com/VickyLi2021) and [David Nissimoff](https://github.com/davidni) who appear to have developed the core engine as part of a Microsoft Hackathon.  

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

## Contributing


This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
