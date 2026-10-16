# lokqlmcp

An [MCP](https://modelcontextprotocol.io) server that lets an AI client query Parquet data with KQL, using the KustoLoco engine.

## Usage

```
lokqlmcp run --folder <folder-containing-parquet-files> [--config <file.md>] [--log <file>]
lokqlmcp updateConfig --folder <folder-containing-parquet-files> --config <file.md>
```

The first argument is a verb:

- `run` starts the server. It loads every `*.parquet` file in the folder as a table, named after its file without the extension, and talks to the client over stdio.
- `updateConfig` reads the tables in the folder and adds an empty entry to the config file for each table that doesn't have one. Existing text is preserved. The file is created if it doesn't exist.

## Configuration file

The optional config file (`--config`) is a simple markdown file:

```markdown
# Context
Overall description of the data. Can span several lines.

# Tables

## orders
One row per customer order. `total` is in USD.

## customers
Customer master data.
```

- `# Context` is appended to the built-in server description and sent to the client as the server instructions. If it is blank or missing, only the built-in description is used. Some MCP clients ignore server instructions.
- Under `# Tables`, each `## <tableName>` heading (case-insensitive) starts a description for that table. The text is included in the `DescribeTable` response. Not every table needs an entry.
- Other headings and text outside these sections are ignored. Lines inside ``` code fences are never treated as headings.
- At startup, the server warns on stderr about entries that match no loaded table.

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
	  "--", "run", "--folder", "C:/data/parquet", "--config", "C:/data/tables.md"]
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
	  "args": ["run", "--folder", "C:/data/parquet", "--config", "C:/data/tables.md"]
	}
  }
}
```
