_⚠ This is a hackathon project with no support or quality guarantee_

# BabyKusto

Welcome to BabyKusto. BabyKusto is a self-contained KQL (Kusto) engine
with partial support for the [Kusto language](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/).

## Example usage

```cs
 // Get your source data from somewhere.
 // Could be from a file on disk, hardcoded in memory, a real database, etc.
ITableSource myTable = /*...*/;

var engine = new BabyKustoEngine();
engine.AddGlobalTable("MyTable", myTable);

var result = engine.Evaluate(@"
    MyTable
    | summarize numSamples = count(),
                v = avg(CounterValue/100)
                by AppMachine");

result.Dump(Console.Out);
```

## Example output from `BabyKusto.Cli`:

```
Welcome to BabyKusto, the little self-contained Kusto execution engine!
-----------------------------------------------------------------------

MyTable:
    AppMachine; CounterName; CounterValue;
    ------------------
    vm0; cpu; 50;
    vm0; mem; 30;
    vm1; cpu; 20;
    vm1; mem; 5;
    vm2; cpu; 100;

Query:

let c=100.0;
MyTable
| where AppMachine != 'vm1'
| project frac=CounterValue/c, AppMachine, CounterName
| summarize avg(frac) by CounterName
| project CounterName, avgRoundedPercent=tolong(avg_frac*100)


Result:
    CounterName; avgRoundedPercent;
    ------------------
    cpu; 75;
    mem; 30;
```

## A simple UI that you can upload your csv file and query against it:
![image](https://user-images.githubusercontent.com/92544828/137518750-0eee9b0f-1cb4-4c28-a72e-57d5e96bf429.png)

To bring up the APP, just build the solution, and run dotnet watch run inside BabyKusto.BlazerApp directory:
![image](https://user-images.githubusercontent.com/92544828/137518974-3cba9d4f-dbcc-4d52-abde-1d99e34107f7.png)

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
