# lokqlmcp

An [MCP](https://modelcontextprotocol.io) server that lets an AI client query Parquet data with KQL, using the KustoLoco engine.

## Usage

```
lokqlmcp [--folder] <folder-containing-parquet-files> [--log <file>]
```

At startup the server loads every `*.parquet` file in the folder as a table. Each table is named after its file, without the extension. The server talks to the client over stdio.

## Request log

Every tool call is appended to a human-readable log file, `lokqlmcp.log` in the current directory by default (override with `--log <file>`). Each entry has a local timestamp, the tool name, its input, the action taken and the duration:

```
[2025-01-15 14:03:22.417] RunQuery
  Input:
    StormEvents | summarize count() by State | render columnchart
  Action: Rendered PNG chart (columnchart, 51 rows, 18 KB)
  Duration: 42 ms
```

## Tools

| Tool | Description |
|------|-------------|
| `ListTables` | Lists the loaded tables with their row and column counts |
| `DescribeTable` | Shows a table's schema and a few sample rows (CSV) |
| `RunQuery` | Runs a KQL query. Returns an error message, a PNG chart (for `render`) or CSV (limited by `maxRows`) |
| `GetKqlHelp` | Explains how KustoLoco differs from ADX, with an optional topic filter |

## Client configuration

VS Code (`.vscode/mcp.json`):

```json
{
  "servers": {
	"lokql": {
	  "type": "stdio",
	  "command": "dotnet",
	  "args": ["run", "--project", "applications/lokqlmcp", "--", "C:/data/parquet"]
	}
  }
}
```

Claude Desktop (`claude_desktop_config.json`):

```json
{
  "mcpServers": {
	"lokql": {
	  "command": "C:/tools/lokqlmcp/lokqlmcp.exe",
	  "args": ["C:/data/parquet"]
	}
  }
}
```
