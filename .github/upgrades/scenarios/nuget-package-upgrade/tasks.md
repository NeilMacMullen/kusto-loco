# NuGet Package Upgrade Progress

**Progress**: 2/9 tasks complete <progress value="22" max="100"></progress> 22%
**Status**: Not Started
Upgrading all outdated NuGet packages across KustoLoco.sln (CPM via Directory.Packages.props) in category batches, skipping CsvHelper and Parquet.Net due to major breaking changes.

**Progress**: 0/9 tasks complete <progress value="0" max="100"></progress> 0%
- ✅ 01-test-infra-packages: test-infra-packages ([Content](tasks/01-test-infra-packages/task.md), [Progress](tasks/01-test-infra-packages/progress-details.md))
- ✅ 02-core-runtime-packages: core-runtime-packages ([Content](tasks/02-core-runtime-packages/task.md), [Progress](tasks/02-core-runtime-packages/progress-details.md))

- 🔲 01-test-infra-packages: Update test infrastructure packages
- 🔲 02-core-runtime-packages: Update core .NET/Microsoft.Extensions packages
- 🔲 03-ai-packages: Update AI-related packages
- 🔲 04-data-parsing-packages: Update data/parsing packages (skip CsvHelper, Parquet.Net)
- 🔲 05-avalonia-ui-packages: Update Avalonia UI packages
- 🔲 06-wpf-and-misc-ui-packages: Update WPF and misc UI packages
- 🔲 07-azure-cloud-packages: Update Azure/cloud and remaining packages
- 🔲 08-divergent-and-skipped-packages: Resolve divergent/skipped packages
- 🔲 09-final-validation: Final build/test validation and summary
