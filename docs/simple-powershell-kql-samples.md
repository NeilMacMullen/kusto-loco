# Simple PSKQL PowerShell Samples

The `pskql` application is a PowerShell cmdlet that lets you use KQL (Kusto Query Language) to query PowerShell objects. Here are simple examples to get you started:

## Setup
First, import the module:
```powershell
Import-Module .\pskql.dll
```

## Basic Examples

### 1. List Object Properties
See what properties are available in your data:
```powershell
ls | edit-kql
```

### 2. Select and Sort Files
Get the 3 largest files by size:
```powershell
ls | edit-kql "project Name,Length | order by Length desc | take 3"
```

### 3. Filter Files
Show only files with extensions:
```powershell
ls | edit-kql "where Extension != ''"
```

### 4. Count Files by Extension
Group files by their extension and show total size:
```powershell
ls | edit-kql "where Extension != '' | summarize sum(Length) by Extension"
```

### 5. Time-based Analysis
Count files by week of last access:
```powershell
ls | edit-kql "summarize count() by bin(LastAccessTime,7d)"
```

## Process Examples

### 6. Running Processes
See current processes:
```powershell
Get-Process | edit-kql "take 5"
```

### 7. Memory Usage
Find processes using most memory:
```powershell
Get-Process | edit-kql "project Name,WorkingSet | order by WorkingSet desc | take 5"
```

### 8. Process Count by Name
Count processes by name:
```powershell
Get-Process | edit-kql "summarize count() by Name"
```

## Service Examples

### 9. Running Services
Show running services:
```powershell
Get-Service | edit-kql "where Status == 'Running' | take 5"
```

### 10. Service Status Summary
Count services by status:
```powershell
Get-Service | edit-kql "summarize count() by Status"
```

## Event Log Examples

### 11. Recent Events
Get recent system events:
```powershell
Get-EventLog System -Newest 100 | edit-kql "take 5"
```

### 12. Error Events
Count error events by source:
```powershell
Get-EventLog System -Newest 1000 | edit-kql "where EntryType == 'Error' | summarize count() by Source"
```

## Advanced Examples

### 13. Custom Categories
Categorize files by size:
```powershell
ls | edit-kql -noqueryprefix "let sz = (s:long) {case (isnull(s),'-',s < 1000,'s',s<1000000,'m','l')} ; data | project Name,Length,Size=sz(Length)"
```

### 14. Generate Date Range
Create folders for the last 10 days:
```powershell
edit-kql -noqueryprefix "range N from 1d to 10d step 1d | extend D=now()-N | project T=format_datetime(D,'yyyy-MM-dd')" | % {New-Item $_.T -Type Directory }
```

### 15. Registry Keys
Analyze registry keys in HKCU:
```powershell
Get-ChildItem HKCU:\Software | edit-kql "take 10"
```

## Visualization Examples

### 16. Pie Chart
Create a pie chart of file sizes:
```powershell
ls | edit-kql "project Name,Length | order by Length | take 10 | render piechart"
```

### 17. Bar Chart
Show process memory usage as bar chart:
```powershell
Get-Process | edit-kql "project Name,WorkingSet | order by WorkingSet desc | take 10 | render barchart"
```

## Tips
- Use `take N` to limit results
- Use `project` to select specific columns
- Use `where` for filtering
- Use `summarize` for aggregation
- Use `order by` for sorting
- Use `render` for visualizations
- The `-noqueryprefix` flag allows more complex queries

These examples show how to use KQL's powerful querying capabilities directly on PowerShell objects, making data analysis much more flexible and powerful than traditional PowerShell commands alone.