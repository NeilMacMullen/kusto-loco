# Progress: 02-core-runtime-packages

## Changes Made
Updated `Directory.Packages.props`:
- `System.Security.Cryptography.Xml`: 10.0.11 → 10.0.14
- `Microsoft.Extensions.Hosting`: 10.0.11 → 10.0.14
- `Microsoft.Extensions.Logging`: 10.0.11 → 10.0.14
- `Microsoft.NET.Sdk.WebAssembly.Pack`: 10.0.11 → 10.0.14
- `System.ServiceProcess.ServiceController`: 10.0.11 → 10.0.14
- `Microsoft.Extensions.DependencyInjection`: 10.0.12 → 10.0.14
- `Microsoft.VisualStudio.Threading`: 18.7.23 → 18.12.1
- `Microsoft.Extensions.Diagnostics.Testing`: 10.10.0 → 10.11.0

`Microsoft.AspNetCore.Components.WebAssembly` and `Microsoft.AspNetCore.Components.WebAssembly.DevServer` (Blazor WASM project packages) were already at the recommended version 10.0.14 — no change needed.

## Build/Test Results
- Full solution build: succeeded, 0 errors.
- Full `dotnet test` run: all test projects passed, 0 failures.

## Issues Resolved
None — all updates were non-breaking per assessment.
