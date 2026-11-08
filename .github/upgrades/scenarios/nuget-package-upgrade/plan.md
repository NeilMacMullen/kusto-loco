# NuGet Package Upgrade Plan

## Overview

**Target**: All NuGet packages with available updates across KustoLoco.sln (Central Package Management via Directory.Packages.props)
**Scope**: Whole solution, ~43 projects, ~110 distinct packages tracked in Directory.Packages.props

## Tasks

### 01-test-infra-packages

Update test-infrastructure packages: `Microsoft.NET.Test.Sdk` (18.9.0→18.11.0), `MSTest.TestFramework`/`MSTest.TestAdapter` (4.4.0→4.5.2), `xunit.runner.visualstudio` (3.1.5→4.0.1), `xunit.v3` (3.2.2→4.0.2), `coverlet.collector` (10.1.0→10.1.0 if changed), `AwesomeAssertions` (confirm current), `Moq` (4.20.72→4.21.0). No breaking API changes reported for these.

**Done when**: Directory.Packages.props updated, solution builds, all test projects build and tests pass.

---

### 02-core-runtime-packages

Update core .NET/Microsoft.Extensions packages: `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Logging`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Diagnostics.Testing`, `Microsoft.AspNetCore.Components.WebAssembly`, `Microsoft.AspNetCore.Components.WebAssembly.DevServer`, `Microsoft.NET.Sdk.WebAssembly.Pack`, `System.Security.Cryptography.Xml`, `System.ServiceProcess.ServiceController`, `Microsoft.VisualStudio.Threading`. No breaking changes reported.

**Done when**: Directory.Packages.props updated (including Blazor WASM project), solution builds, tests pass.

---

### 03-ai-packages

Update AI-related packages: `Microsoft.Extensions.AI`, `Microsoft.Extensions.AI.Abstractions`, `Microsoft.Extensions.AI.OpenAI`, `OpenAI`, `ModelContextProtocol`, `Anthropic.SDK`. `Microsoft.Extensions.AI.OpenAI` and `OpenAI` have breaking API changes reported — attempt small fixes to call sites; revert both together if not feasible without large rework.

**Done when**: Packages updated where feasible, affected call sites compile, tests pass. Any package reverted is documented with reason.

---

### 04-data-parsing-packages

Update data/parsing packages excluding the two flagged breaking majors: `ClosedXML`, `ExcelDataReader.DataSet`, `ShapeCrawler`, `Magick.NET-Q16-AnyCPU`, `Newtonsoft.Json`, `Snappier`, `Tmds.DBus.Protocol`, `SkiaSharp.HarfBuzz` (if referenced), `geohash-dotnet`, `Microsoft.Azure.Kusto.Data`, `Microsoft.Azure.Kusto.Language`. **Skip** `CsvHelper` (33.1.0→34.0.0, major breaking) and `Parquet.Net` (5.6.0→6.1.0, major breaking) — leave at current pinned versions.

**Done when**: Non-breaking packages updated, CsvHelper and Parquet.Net left unchanged, solution builds, tests pass.

---

### 05-avalonia-ui-packages

Update Avalonia and related desktop UI packages: `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`, `Avalonia.Skia`, `Avalonia.AvaloniaEdit`, `AvaloniaEdit.TextMate`, `Avalonia.Headless.XUnit`, `Avalonia.Labs.Controls`, `Avalonia.Labs.Panels`, `TreeDataGrid`, `Dock.Avalonia`, `Dock.Avalonia.Themes.Fluent`, `Dock.Model.Mvvm`, `Clowd.Clipboard.Avalonia`, `MessageBox.Avalonia`, `Xaml.Behaviors.Avalonia`, `Svg.Controls.Avalonia`, `Mapsui.Avalonia12`, `HotAvalonia`, `StaticViewLocator`.

**Done when**: Directory.Packages.props updated, Avalonia-based projects build, tests pass.

---

### 06-wpf-and-misc-ui-packages

Update remaining UI / misc packages: `WPF-UI`, `ScottPlot`, `ScottPlot.Avalonia`, `ScottPlot.WPF`, `Topten.RichTextKit`, `MarkdownViewer.Core`, `Markdig.Wpf` (if referenced), `CommunityToolkit.HighPerformance`, `CommunityToolkit.Mvvm`, `DependencyPropertyGenerator`, `Jab`, `Spectre.Console`.

**Done when**: Directory.Packages.props updated, build succeeds, tests pass.

---

### 07-azure-cloud-packages

Update Azure/cloud-related packages: `Azure.AI.OpenAI`, `Azure.Identity`, `Azure.Monitor.Query.Logs`, `Swashbuckle.AspNetCore`, `Microsoft.PowerShell.SDK`, `System.CommandLine`, `BenchmarkDotNet`, `NCrunch.Framework`, `NLog`, `NLog.Extensions.Logging`, `NLog.Layouts.ClefJsonLayout`, `CommandLineParser`, `McMaster.NETCore.Plugins`, `Microsoft.CodeAnalysis.Analyzers`, `Microsoft.CodeAnalysis.CSharp`, `TestableIO.System.IO.Abstractions` (+ Wrappers/TestingHelpers), `VegaGenerator`, `morelinq`, `NotNullStrings`, `Vanara.Core`, `Vanara.PInvoke.NetApi32`, `Fastenshtein`, `SimpleExec`, `T-Digest.NET`, `System.Linq.Async`.

**Done when**: Directory.Packages.props updated, build succeeds, tests pass.

---

### 08-divergent-and-skipped-packages

Resolve `Blazored.LocalStorage` divergence flagged by assessment (Blazor-Server.csproj: no supported version found above 4.5.0 for that project) — keep pinned at current version and document as skipped. Confirm `CsvHelper` and `Parquet.Net` remain at their pre-upgrade versions with documented reasons.

**Done when**: scenario-instructions.md and final summary clearly list all skipped/reverted packages with reasons.

---

### 09-final-validation

Run a full solution build and complete test suite after all prior tasks to confirm overall health, and produce the final upgrade summary (updated vs skipped packages with old→new versions).

**Done when**: Full solution builds with no errors, all tests pass, summary delivered to user.
