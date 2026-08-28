# KustoLoco.UserAgent

An optional user-agent parser for KustoLoco's `parse_user_agent()` function.

KustoLoco's core engine implements `parse_user_agent()` natively but ships **no** user-agent
dataset — the function reads from an `IUserAgentParser` that the host registers. This package
is one such provider, backed by the canonical
[ua-parser / uap-core](https://github.com/ua-parser/uap-core) dataset (`regexes.yaml`,
Apache-2.0), which is embedded in the assembly. The parsing cascade is an original,
dependency-free implementation of the uap-core specification.

## Usage

```csharp
using KustoLoco.Core;
using KustoLoco.UserAgent;

var context = new KustoQueryContext();
context.AddProvider<IUserAgentParser>(UapUserAgentParser.Default);

var result = await context.RunQuery(
    "print ua = parse_user_agent('Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
    "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36')");
```

`UapUserAgentParser.Default` is a shared, immutable instance built from the embedded dataset,
safe to reuse across concurrent queries. You can also construct one from a newer
`regexes.yaml` stream via `new UapUserAgentParser(stream)`.

### Output shape

`parse_user_agent()` returns a dynamic object with the uap-core structure:

```json
{
  "Browser":         { "Family": "...", "Major": "...", "Minor": "...", "Patch": "..." },
  "OperatingSystem": { "Family": "...", "Major": "...", "Minor": "...", "Patch": "...", "PatchMinor": "..." },
  "Device":          { "Family": "...", "Brand": "...", "Model": "..." }
}
```

Unmatched components resolve to `Family: "Other"`, exactly as uap-core specifies.

## Attribution

The embedded dataset is © the ua-parser contributors, licensed Apache-2.0. See [`NOTICE`](NOTICE).
