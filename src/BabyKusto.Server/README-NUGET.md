# BabyKusto.Server

An implementation of the Kusto REST API protocol (v1 and v2). Use this library to create a server that Azure Data Explorer can connect to as if it was a real Kusto cluster.


## How to use
Use the provided [**Sample.ProcessesServer**](https://github.com/davidnx/baby-kusto-csharp/tree/main/samples/Sample.ProcessesServer) as inspiration.
This sample is an ASP .NET Core-based web server that implements the Kusto REST API and exposes a table `Processes` that shows the list of running processes on your environment. You can connect to the local Kusto cluster using the official Kusto client (Azure Data Explorer).

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
