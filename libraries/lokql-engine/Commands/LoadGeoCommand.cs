using System.Reflection.Metadata;
using CommandLine;
using Kusto.Cloud.Platform.Utils;
using KustoLoco.Core;
using KustoLoco.Geo;
using KustoLoco.PluginSupport;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class LoadGeoCommand
{
    private const string DefaultDb = "default";
    private static string _lastLoaded = "database not yet loaded";
    internal static Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;

        var wanted = o.FileName.OrWhenBlank(DefaultDb);
        if (_lastLoaded == o.FileName && !o.Force)
        {
            console.Warn($"'{wanted}' is already loaded.  Use the '-f' option to force reload");
        }
        else
        {
            var queryContext = context.QueryContext;
            console.Info($"Loading {wanted}... this may take some time");
            var p = wanted.EqualsOrdinalIgnoreCase(DefaultDb)
                ? DbIpGeoProvider.Default
                : DbIpGeoProvider.FromFile(o.FileName);
            queryContext.AddProvider<IGeoIpProvider>(p);
            _lastLoaded = wanted;
            console.Info($"Loaded {wanted}");
            console.Warn("""
                         IP Geolocation by DB-IP (https://db-ip.com)
                         Country centroids (C) Google (https://github.com/google/dspl), CC-BY-4.0
                         """);
        }
       
        return Task.CompletedTask;
    }

    [Verb("loadgeoip", HelpText =
        """
        Loads a geo-ip database to allow the geo_info_from_ip_address to return valid info.
        If no filename is supplied, the in-built country-level data is used.
        Files may be csv or csv.gz and are provided by https://db-ip.com
        under the Creative Commons Attribution 4.0 International License (CC-BY-4.0).

        Examples:
           .loadgeoip
           .loadgeoip default
           .loadgeoip "C:\Users\User\Downloads\dbip-city-lite-2026-08.csv.gz"
        """)]
    internal class Options
    {
        [Value(0, HelpText = "table name", Required = false)]
        public string FileName { get; set; } = string.Empty;
        [Option(HelpText = "force reload", Required = false)]
        public bool Force { get; set; } 


    }
}
