using CommandLine;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;
using KustoLoco.PluginSupport;
using KustoLoco.Core;

namespace LokqlPlugin
{
    internal class LokqlPlugin : ILokqlCommand
    {
        public string GetNameAndVersion() => "processes - v1";

        public ICommandProcessor Register(ICommandProcessor processor)
            => processor.WithAdditionalCommand<Options>(RunAsync);


        private static async Task RunAsync(ICommandContext context, Options o)
        {
            var console = context.Console;
            var queryContext = context.QueryContext;
            var blocks = context.InputProcessor;
            var settings = context.Settings;
            var history = context.History;
            var tableName = o.TableName;
            console.Info("Reading processes...");
            var processes = ProcessReader.GetProcesses();
            var table = TableBuilder.CreateFromImmutableData(tableName, processes);
            queryContext.AddTable(table);

            //now show the table by running a query against it...
            await context.RunInput(tableName);
            console.Info($"Processes are now available in table '{tableName}'");
        }
    }

    [Verb("processes", HelpText = """
                                  Creates a new table containing current process information.
                                  By default the table is called 'processes' but this can be overridden
                                  by supplying a different name

                                  Examples:

                                  .processes 

                                  .processes proc
                                  """)]
    internal class Options
    {
        [Value(0, HelpText = "Name of table", Required = false)]
        public string TableName { get; set; } = "processes";
    }
}
