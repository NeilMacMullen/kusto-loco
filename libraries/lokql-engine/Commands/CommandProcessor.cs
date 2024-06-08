using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using KustoLoco.Core.Console;

namespace Lokql.Engine.Commands
{
    public class CommandProcessor
    {

        public async Task RunInternalCommand(InteractiveTableExplorer exp, string currentLine, BlockSequence sequence)
        {
            var splitter = CommandLineStringSplitter.Instance;
            var tokens = splitter.Split(currentLine).ToArray();

            if (!tokens.Any())
                return;




            var textWriter = new StringWriter();
            await StandardParsers.CreateWithHelpWriter(textWriter)
                    .ParseArguments(tokens,
                        typeof(ExitCommand.Options),
                        typeof(SaveCommand.Options),
                        typeof(RenderCommand.Options),
                        typeof(LoadCommand.Options),
                        typeof(FormatCommand.Options),
                        typeof(RunScriptCommand.Options),
                        typeof(QueryCommand.Options),
                        typeof(SaveQueryCommand.Options),
                        typeof(MaterializeCommand.Options),
                        typeof(SynTableCommand.Options),
                        typeof(AllTablesCommand.Options),
                        typeof(ShowCommand.Options),
                        typeof(FileFormatsCommand.Options),
                        typeof(SetCommand.Options),
                        typeof(ListSettingsCommand.Options),
                        typeof(ListSettingDefinitionsCommand.Options),
                        typeof(AppInsightsCommand.Options),
                        typeof(PivotCommand.Options)
                    )
                    .WithParsed<PivotCommand.Options>(o => PivotCommand.Run(exp, o))
                    .WithParsed<MaterializeCommand.Options>(o => MaterializeCommand.Run(exp, o))
                    .WithParsed<RenderCommand.Options>(o => RenderCommand.Run(exp, o))
                    .WithParsed<AllTablesCommand.Options>(o => AllTablesCommand.Run(exp, o))
                    .WithParsed<ExitCommand.Options>(o => ExitCommand.Run(exp, o))
                    .WithParsed<FormatCommand.Options>(o => FormatCommand.Run(exp, o))
                    .WithParsed<SynTableCommand.Options>(o => SynTableCommand.Run(exp, o))
                    .WithParsedAsync<RunScriptCommand.Options>(o => RunScriptCommand.RunAsync(exp, o))
                    .WithParsedAsync<SaveQueryCommand.Options>(o => SaveQueryCommand.RunAsync(exp, o))
                    .WithParsedAsync<LoadCommand.Options>(o => LoadCommand.RunAsync(exp, o))
                    .WithParsedAsync<SaveCommand.Options>(o => SaveCommand.RunAsync(exp, o))
                    .WithParsedAsync<QueryCommand.Options>(o => QueryCommand.RunAsync(exp, o))
                    .WithParsedAsync<ShowCommand.Options>(o => ShowCommand.RunAsync(exp, o))
                    .WithParsedAsync<FileFormatsCommand.Options>(o => FileFormatsCommand.RunAsync(exp, o))
                    .WithParsedAsync<SetCommand.Options>(o => SetCommand.RunAsync(exp, o))
                    .WithParsedAsync<ListSettingsCommand.Options>(o => ListSettingsCommand.RunAsync(exp, o))
                    .WithParsedAsync<ListSettingDefinitionsCommand.Options>(o =>
                        ListSettingDefinitionsCommand.RunAsync(exp, o))
                    .WithParsedAsync<AppInsightsCommand.Options>(o => AppInsightsCommand.RunAsync(exp, o, sequence))
                ;
            exp.Info(textWriter.ToString());
        }
    }
}
