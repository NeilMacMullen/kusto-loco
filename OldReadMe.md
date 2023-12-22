# DO NOT USE

This fork is currently undergoing changes and is unstable.
Please refer back to the original repo for a more stable version

# BabyKusto

BabyKusto is a self-contained execution engine for the [Kusto Query Language](https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/) (KQL).


## How to use

1. Evaluate a simple query:

   ```cs
   var query = "print hello='world'";
   
   var engine = new BabyKustoEngine();
   var result = engine.Evaluate(query);
   ```

2. Inject custom tabular sources:

   Implement an `ITableSource`, then register it with the engine using `BabyKustoEngine.AddGlobalTable(ITableSource table)`. You can think of `ITableSource` as similar to an `IEnumerable<T>`, which allows the engine to get metadata about the table (e.g. its name and type) as well as to iterate over its data.

   Just like `IEnumerable<T>`, the data doesn't have to be materialized ahead of time, and your implementation of `ITableSource` can produce data on the fly. The type, however, has to be static and known ahead of time.

   ```cs
   ITableSource myTable = /*...*/; // Get your data from anywhere
   var engine = new BabyKustoEngine();
   engine.AddGlobalTable(myTable);
   
   var result = engine.Evaluate("MyTable | count");
   ```

3. Play with the samples

   This repo ships with three ready-to-run samples that showcase BabyKusto in action.
   
   * [**Sample.HelloWorld**](./samples/Sample.HelloWorld): as simple as it gets, shows how to run a simple query.
   
   * [**Sample.ProcessesCli**](./samples/Sample.ProcessesCli): a command-line tool that lets you explore processes running on your machine using KQL. For example, find the process using the most memory with a query like this:
     ```
     Processes
     | project name, memMB=workingSet/1024/1024
     | order by memMB desc
     | take 1
     ```

   * [**Sample.ProcessesServer**](./samples/Sample.ProcessesServer): an ASP .NET Core-based web server that implements the Kusto REST API and exposes the same table `Processes` as the `Sample.ProcessesCli` sample. You can connect to the local Kusto cluster using the official Kusto client (Azure Data Explorer).

## How it works

BabyKusto leverages the official [`Microsoft.Azure.Kusto.Language`](https://www.nuget.org/packages/Microsoft.Azure.Kusto.Language/) package for parsing and semantic analysis of KQL queries.

The syntax tree is then translated to BabyKusto's internal representation (see [InternalRepresentation](./src/BabyKusto.Core/InternalRepresentation)), which is evaluated by [BabyKustoEvaluator.cs](./src/BabyKusto.Core/Evaluation/BabyKustoEvaluator.cs).

You can explore the internal representation of a query by setting `dumpIRTree: true` when calling `BabyKustoEngine.Evaluate`.
Below is an example of the internal representation for the query:

```
Processes
| project name, memMB=workingSet/1024/1024
| order by memMB desc
| take 1
```

![Internal representation outputs](./docs/internal-representation.png)

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
