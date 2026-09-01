using CommandLine;
using Kusto.Cloud.Platform.Utils;
using KustoLoco.Core;
using KustoLoco.Geo;
using KustoLoco.PluginSupport;
using KustoLoco.UserAgent;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class LoadUserAgentCommand
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
            console.Info($"Loading {wanted}...");
            var p = wanted.EqualsOrdinalIgnoreCase(DefaultDb)
                ? UapUserAgentParser.Default
                : UapUserAgentParser.FromFile(o.FileName);
            queryContext.AddProvider<IUserAgentParser>(p);
            _lastLoaded = wanted;
            console.Info($"Loaded {wanted}");
            console.Warn("""
                         Copyright ua-parser contributors.
                         Licensed under the Apache License, Version 2.0 (the "License");
                         you may not use this file except in compliance with the License.
                         You may obtain a copy of the License at
                             https://www.apache.org/licenses/LICENSE-2.0
                         Unless required by applicable law or agreed to in writing, software distributed under
                         the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
                         KIND, either express or implied. See the License for the specific language governing
                         permissions and limitations under the License.
                         """);
        }

        return Task.CompletedTask;
    }

    [Verb("loaduseragents", HelpText =
        """
        Loads a user-agent database to allow the parse_user_agent command to return valid info.
        If no filename is supplied, the in-built database is used.  This is taken from 
        https://github.com/ua-parser/uap-core/blob/master/regexes.yaml
        
        Copyright ua-parser contributors.
        Licensed under the Apache License, Version 2.0 (the "License");
        you may not use this file except in compliance with the License.
        You may obtain a copy of the License at
            https://www.apache.org/licenses/LICENSE-2.0
        Unless required by applicable law or agreed to in writing, software distributed under
        the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
        KIND, either express or implied. See the License for the specific language governing
        permissions and limitations under the License.

        Examples:
           .loaduseragents
           .loaduseragents default
           .loaduseragents "C:\Users\User\Downloads\regexes.yaml"
        """)]
    internal class Options
    {
        [Value(0, HelpText = "regex file", Required = false)]
        public string FileName { get; set; } = string.Empty;
        [Option(HelpText = "force reload", Required = false)]
        public bool Force { get; set; }
    }
}
