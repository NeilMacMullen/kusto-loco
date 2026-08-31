using System.Reflection;
using CliRendering;
using KustoLoco.Core;
using KustoLoco.Geo;
using NotNullStrings;
using Spectre.Console;

ShowHelpIfAppropriate(false);
var renderingPreference = CliRenderer.GetValidatedRenderingPreference(args, 2);
if (renderingPreference.IsBlank())
    ShowHelpIfAppropriate(true);

var renderer = new CliRenderer(renderingPreference);
var context = new KustoQueryContext();
var dbName = args[0];
Console.WriteLine($"Loading db from '{dbName}'...");
if (dbName != "default")
    context.AddProvider<IGeoIpProvider>(DbIpGeoProvider.FromFile(dbName));
else
    context.AddProvider<IGeoIpProvider>(DbIpGeoProvider.Default);
Console.WriteLine("Loaded db.");
Console.WriteLine("""
                  IP Geolocation by DB-IP (https://db-ip.com)
                  Country centroids (C) Google (https://github.com/google/dspl), CC-BY-4.0
                  """);
while (true)
{
    Console.Write("Enter an IP: ");
    var ip = Console.ReadLine()!.Trim();
    var result = await context.RunQuery($"""
                                         print tostring(geo_info_from_ip_address('{ip}'))
                                         """);
    renderer.DisplayQueryResult(result);
}


void ShowHelpIfAppropriate(bool force)
{
    if (!force && (args.Length is > 0 and < 3)) return;
    var programName = $"{Assembly.GetExecutingAssembly().GetName().Name}.exe";
    var help = $"""
                This program demonstrates the use of geo_info_from_ip_address.
                A path to a DB-IP database file must be provided or the path "default" may be used to specify
                the in-built country-level database.
                Usage:
                 {programName} default 
                 {programName} "C:\Users\User\Downloads\dbip-city-lite-2026-08.csv.gz" 
                 
                If two arguments are specified the first is interpreted as directive to control chart rendering.
                 
                """;
    help += CliRenderer.PreferencesHelp;
    AnsiConsole.MarkupLineInterpolated($"[yellow]{help}[/]");
    Environment.Exit(0);
}
