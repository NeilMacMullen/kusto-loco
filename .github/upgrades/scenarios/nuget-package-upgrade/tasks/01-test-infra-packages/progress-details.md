# Progress: 01-test-infra-packages

## Changes Made
- Updated `Directory.Packages.props`:
  - `Microsoft.NET.Test.Sdk`: 18.9.0 → 18.11.0
  - `MSTest.TestFramework`: 4.4.0 → 4.5.2
  - `MSTest.TestAdapter`: 4.4.0 → 4.5.2
  - `Moq`: 4.20.72 → 4.21.0
- `xunit.v3` and `xunit.runner.visualstudio` were initially bumped to 4.0.2/4.0.1 but **reverted** back to 3.2.2/3.1.5 (kept at current) after discovering a breaking ABI change: `Avalonia.Headless.XUnit` (pinned at 12.1.5) calls `Xunit.v3.TestIntrospectionHelper.GetTestCaseDetails` with a signature that no longer exists in xunit.v3 4.0.2, causing `MissingMethodException` at test discovery time in `lokqlDxComponentsTests`. Since no compatible `Avalonia.Headless.XUnit` version is available to pair with the newer xunit.v3, this update was skipped.
- `AwesomeAssertions` and `coverlet.collector` were already at their recommended versions — no change needed.

## Build/Test Results
- Full solution build: succeeded, 0 errors, 0 warnings.
- Full `dotnet test` run across solution: all projects passed (0 failures), including `lokqlDxComponentsTests` (33/33 passed).

## Issues Resolved
- Reverted xunit.v3 / xunit.runner.visualstudio bump due to breaking change with Avalonia.Headless.XUnit (documented above).
