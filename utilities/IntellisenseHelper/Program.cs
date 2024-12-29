using System.Text;
using System.Text.Json;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;


var debug = true;
var allHelps = new List<Help>();
var folder = args[0];
var filter =  (args.Length>1) ? args[1] : string.Empty;
var engine = new BabyKustoEngine(new NullConsole(), new KustoSettingsProvider());

var funcs = engine.GetImplementedList()
    .Where(f=>f.Implemented)
    .Select(f=>f.Name)
    .Distinct().ToHashSet();

foreach (var f in Directory.EnumerateFiles(folder, "*.md"))
{
    if(!f.Contains(filter))
        continue;
    if (debug)
    Console.WriteLine($"Checking function {f}");
    var lines = File.ReadAllLines(f);
    var state = "starting";
    var name = "";
    var description = new StringBuilder();
    var syntax = new StringBuilder();
    foreach (var line in lines)
    {
        if(debug)
            Console.WriteLine($"{state} : {line}");
        if (state == "starting")
        {
            if (line.StartsWith("#"))
            {
                name = line.Substring(1)
                    .Replace("()", "")
                    .Replace("operator","")
                    .Trim();
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
            if (filter.Length!=0 || funcs.Contains(name))
            {
                allHelps.Add(new Help(name, description.ToString().Trim(), syntax.ToString().Trim()));
                Console.WriteLine($"Name:'{name}'");
                Console.WriteLine($"Description:'{description.ToString().Trim()}'");
                Console.WriteLine($"Syntax:'{syntax.ToString().Trim()}'");
            }
            else
                if(debug)
                    Console.WriteLine($"NOT IMPLEMENTED {name}");

                break;
        }
    }
}

foreach(var func in funcs)
{
    if (!allHelps.Any(h => h.Name == func))
    {
        Console.WriteLine($"MISSING {func}");
    }
}
Console.WriteLine("Done");

var json = JsonSerializer.Serialize(allHelps,new JsonSerializerOptions(){WriteIndented = true});

File.WriteAllText("functions.json", json);
Console.WriteLine("Written to functions.json");

public readonly record struct Help(string Name,string Description, string Syntax);
