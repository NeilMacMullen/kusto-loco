# Avalonia 12 migration notes (lokqlDx + lokqlDxComponents)

## Scope
- `applications/lokqlDx`
- `libraries/lokqlDxComponents`

## Package migration summary
- Upgraded core Avalonia packages to `12.1.1`:
  - `Avalonia`
  - `Avalonia.Desktop`
  - `Avalonia.Skia`
  - `Avalonia.Themes.Fluent`
  - `Avalonia.Fonts.Inter`
  - `Avalonia.Headless.XUnit`
- Upgraded related ecosystem packages:
  - `Avalonia.AvaloniaEdit` -> `12.0.0`
  - `AvaloniaEdit.TextMate` -> `12.0.0`
  - `Avalonia.Labs.Controls` -> `12.0.2`
  - `Avalonia.Labs.Panels` -> `12.0.2`
  - `Dock.Avalonia` / `Dock.Avalonia.Themes.Fluent` / `Dock.Model.Avalonia` / `Dock.Model.Mvvm` -> `12.1.0`
  - `Xaml.Behaviors.Avalonia` / `Xaml.Behaviors.Interactions*` -> `12.0.5`
  - `Svg.Controls.Avalonia` -> `12.0.0.13`
  - `MessageBox.Avalonia` -> `12.0.0`
- Replaced TreeDataGrid package reference:
  - removed `Avalonia.Controls.TreeDataGrid`
  - added `TreeDataGrid` version `12.0.0`

## Code/XAML compatibility updates
- Focus events:
  - Replaced `GotFocusEventArgs` handlers with `RoutedEventArgs` handlers.
  - Replaced `OnGotFocus` override in `CopilotDocumentView` with `GotFocus` event subscription.
- Removed Avalonia 11 data validation plugin hook:
  - removed `BindingPlugins.DataValidators.RemoveAt(0)` from `App.axaml.cs` (API no longer public in Avalonia 12).
- Removed debug dev-tools attachment call:
  - removed `AttachDevTools()` call in `DialogService` (Avalonia.Diagnostics package not kept in migration set).
- Bitmap save API:
  - switched to `rtb.Save(memoryStream, PngBitmapEncoderOptions.Default)`.
- XAML deprecations:
  - replaced `TextBox.Watermark` with `PlaceholderText` in `CopilotDocumentView.axaml`.

## TreeDataGrid notes
- NuGet package is now `TreeDataGrid` (v12.0.0), but theme resource URI remains:
  - `avares://Avalonia.Controls.TreeDataGrid/Themes/Fluent.axaml`
- Existing `TreeDataGrid` usage and custom `MyFlatTreeDataGridSource<TModel>` compiled without API rewrites.

## Build validation performed
- `dotnet build applications/lokqlDx/lokqlDx.csproj` (Debug): success
- `dotnet build applications/lokqlDx/lokqlDx.csproj -c Release`: success
- `dotnet build libraries/lokqlDxComponents/lokqlDxComponents.csproj -c Release /p:BuildProjectReferences=false`: success

## Follow-up candidates
- If runtime diagnostics tooling is needed in Debug, reintroduce Avalonia 12-compatible diagnostics integration via current recommended API/package.
