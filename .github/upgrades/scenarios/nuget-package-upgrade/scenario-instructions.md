# NuGet Package Upgrade

## Strategy
Batch updates by package category (test infra, core runtime, AI, data/parsing, Avalonia UI, WPF/misc UI, Azure/cloud), build+test after each batch; skip/revert CsvHelper and Parquet.Net (major breaking changes).

## Preferences
- **Flow Mode**: Automatic
- **Scope**: Whole solution (KustoLoco.sln), Central Package Management (Directory.Packages.props)
- **Packages**: All packages with available updates
- **Version Policy**: Latest stable; skip major/breaking updates causing compile errors (e.g. CsvHelper, Parquet.Net) and revert to prior version
- **Batching**: Small batches, build + test after each batch

## Source Control
- **Source Branch**: nuget20261108
- **Working Branch**: nuget20261108 (current branch, no new branch)
- **Commit Strategy**: After Each Task
- **Branch Sync**: Disabled (working on current branch directly)
