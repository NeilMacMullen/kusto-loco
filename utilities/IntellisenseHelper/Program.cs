using System.Net.Mime;
using System.Text;
using System.Text.Json;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using NotNullStrings;

//
//  Note that the docs can be downloaded as markdown from
//  https://github.com/MicrosoftDocs/dataexplorer-docs
//  and are found in the blob/main/data-explorer/kusto/query
//  folder..
//  https://github.com/MicrosoftDocs/dataexplorer-docs/blob/main/data-explorer/kusto/query
//

var debug = true;
var allHelps = new List<Help>();

var folder = args.FirstOrDefault() ?? string.Empty;
if (folder.IsBlank())
    folder = @"..\..\..\..\..\..\dataexplorer-docs\data-explorer\kusto\query";
var filter = args.Length > 1 ? args[1] : string.Empty;
var engine = new BabyKustoEngine(new NullConsole(), new KustoSettingsProvider());

var implementedFunctions = engine.GetImplementedFunctionList()
    .Where(f => f.Implemented)
    .Select(f => f.Name)
    .Distinct()
    .ToHashSet();
var unimplementedFunctions= engine.GetImplementedFunctionList()
    .Where(f => !f.Implemented)
    .Select(f => f.Name)
    .Distinct()
    .ToHashSet();

var implementedAggregates = engine.GetImplementedAggregateList()
    .Where(f => f.Implemented)
    .Select(f => f.Name)
    .Distinct()
    .ToHashSet(); ;

var unimplementedAggregates = engine.GetImplementedAggregateList()
    .Where(f => !f.Implemented)
    .Select(f => f.Name)
    .Distinct()
    .ToHashSet(); ;


var notFound = new List<string>();

foreach (var f in Directory.EnumerateFiles(folder, "*.md"))
{
    if (!f.Contains("-function.md"))
        continue;
    var name = Path.GetFileName(f).Replace("-function.md","");
    if (!f.Contains(filter))
        continue;
    if (debug)
        Console.WriteLine($"Checking function {f}");
    var lines = File.ReadAllLines(f);
    var state = "starting";
    var description = new StringBuilder();
    var syntax = new StringBuilder();
    foreach (var line in lines)
    {
        if (debug)
            Console.WriteLine($"{state} : {line}");
        if (state == "starting")
        {
            if (line.StartsWith("#"))
            {
                /*
                name = line.Substring(1)
                    .Replace("()", "")
                    .Replace("operator", "")
                    .Trim();
                */
                state = "description";
            }

            continue;
        }

        if (state == "description")
        {
            if (line.StartsWith("#"))
                state = "syntax";
            else
                description.AppendLine(line);
            continue;
        }

        if (state == "syntax")
        {
            if (line.StartsWith("#"))
                state = "finished";
            else
                syntax.AppendLine(line);
            continue;
        }

        if (state == "finished")
        {
            //some of the docs have hyphens where they shouldn't be
            var squashedName = name.Replace("-", "");
            if (implementedFunctions.Contains(squashedName))
                name = squashedName;
            //or maybe they've used hyphens for underscores...
            var flatName = name.Replace("-", "_");
            if (implementedFunctions.Contains(flatName))
                name = flatName;

            if (filter.Length != 0 || implementedFunctions.Contains(name))
            {
                allHelps.Add(new Help(name, description.ToString().Trim(), syntax.ToString().Trim()));
                Console.WriteLine($"Name:'{name}'");
                Console.WriteLine($"Description:'{description.ToString().Trim()}'");
                Console.WriteLine($"Syntax:'{syntax.ToString().Trim()}'");
            }
            else
            {
                notFound.Add(name);
            }

            break;
        }
    }
}

foreach (var func in implementedFunctions)
    if (!allHelps.Any(h => h.Name == func))
        Console.WriteLine($"MISSING FUNCTION {func}");

foreach (var unimplementedFunction in unimplementedFunctions)
{
    Console.WriteLine($"UNIMPLEMENTED FUNCTION {unimplementedFunction}");
}

foreach (var unimplementedFunction in unimplementedAggregates)
{
    Console.WriteLine($"UNIMPLEMENTED AGGREGATE {unimplementedFunction}");
}




Console.WriteLine("Done");

var json = JsonSerializer.Serialize(allHelps, new JsonSerializerOptions { WriteIndented = true });

File.WriteAllText("functions.json", json);
Console.WriteLine("Written to functions.json");

public readonly record struct Help(string Name, string Description, string Syntax);
