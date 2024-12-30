using System.Collections.Immutable;
using System.CommandLine.Parsing;
using System.Reflection;
using CommandLine;

namespace Lokql.Engine.Commands;

public readonly record struct CommandProcessorContext(InteractiveTableExplorer Explorer, BlockSequence Sequence);

public class CommandProcessor
{
    private ImmutableList<RegisteredCommand>  _registrations
        = [];

    private CommandProcessor()
    {
    }

    public static CommandProcessor Default()
    {
        return new CommandProcessor()
                .WithAdditionalCommand<LoadCommand.Options>(LoadCommand.RunAsync)
                .WithAdditionalCommand<SaveCommand.Options>(SaveCommand.RunAsync)
                .WithAdditionalCommand<SetCommand.Options>(SetCommand.RunAsync)
                .WithAdditionalCommand<SettingsCommand.Options>(SettingsCommand.RunAsync)
                .WithAdditionalCommand<KnownSettingsCommand.Options>(KnownSettingsCommand.RunAsync)
                .WithAdditionalCommand<PushCommand.Options>(PushCommand.RunAsync)
                .WithAdditionalCommand<PullCommand.Options>(PullCommand.RunAsync)
                .WithAdditionalCommand<ResultsCommand.Options>(ResultsCommand.RunAsync)
                .WithAdditionalCommand<CsvDataCommand.Options>(CsvDataCommand.RunAsync)
                .WithAdditionalCommand<ListTablesCommand.Options>(ListTablesCommand.Run)
                .WithAdditionalCommand<MaterializeCommand.Options>(MaterializeCommand.Run)
                .WithAdditionalCommand<SynTableCommand.Options>(SynTableCommand.Run)
                .WithAdditionalCommand<FileFormatsCommand.Options>(FileFormatsCommand.RunAsync)
                .WithAdditionalCommand<AppInsightsCommand.Options>(AppInsightsCommand.RunAsync)


                .WithAdditionalCommand<PivotCommand.Options>(PivotCommand.Run)
               
                .WithAdditionalCommand<RenderCommand.Options>(RenderCommand.Run)
                .WithAdditionalCommand<ExitCommand.Options>(ExitCommand.Run)
                .WithAdditionalCommand<FormatCommand.Options>(FormatCommand.Run)
                .WithAdditionalCommand<RunScriptCommand.Options>(RunScriptCommand.RunAsync)
                .WithAdditionalCommand<SaveQueryCommand.Options>(SaveQueryCommand.RunAsync)
                .WithAdditionalCommand<QueryCommand.Options>(QueryCommand.RunAsync)
                .WithAdditionalCommand<DefineMacroCommand.Options>(DefineMacroCommand.RunAsync)
                .WithAdditionalCommand<RunMacroCommand.Options>(RunMacroCommand.RunAsync)
                .WithAdditionalCommand<StartReportCommand.Options>(StartReportCommand.Run)
                .WithAdditionalCommand<RenderToReportCommand.Options>(RenderToReportCommand.Run)
                .WithAdditionalCommand<RenderTableToReportCommand.Options>(RenderTableToReportCommand.Run)
                .WithAdditionalCommand<SleepCommand.Options>(SleepCommand.Run)
                .WithAdditionalCommand<EndReportCommand.Options>(EndReportCommand.Run)
                .WithAdditionalCommand<RenderTableToText.Options>(RenderTableToText.Run)
            ;

    }

    public CommandProcessor WithAdditionalCommand<T>(Func<CommandProcessorContext, T, Task> registration)
    {
        _registrations = _registrations.Add(
            new RegisteredCommand(typeof(T), (exp, o) => registration(exp, (T)o)));
        return this;
    }

    public async Task RunInternalCommand(InteractiveTableExplorer exp, string currentLine, BlockSequence sequence)
    {
        var splitter = CommandLineStringSplitter.Instance;
        var tokens = splitter.Split(currentLine).ToArray();

        if (!tokens.Any())
            return;


        var textWriter = new StringWriter();

        var typeTable = _registrations.Select(r=>r.OptionType).ToArray();

        var parserResult = StandardParsers
            .CreateWithHelpWriter(textWriter)
            .ParseArguments(tokens, typeTable);

        var context = new CommandProcessorContext(exp, sequence);
        foreach (var registration in _registrations)
        {
            async Task Func(object o)
            {
                await registration.TaskGeneratingFunction(context, o);
            }

            await parserResult.TryAsync(registration.OptionType, Func);
        }

        exp.Info(textWriter.ToString());
    }

    public Dictionary<string, string> GetVerbs()
    {
        var verbs= _registrations
            .SelectMany(t => t.OptionType.GetTypeInfo().GetCustomAttributes(typeof(VerbAttribute), true))
            .OfType<VerbAttribute>()
            .ToDictionary(a => a.Name, a => a.HelpText);
        verbs["help"]= @"Shows a list of available commands or help for a specific command
.help            for a summary of all commands
.help *command*  for details of a specific command";
        return verbs;
    }

    private readonly record struct RegisteredCommand(
        Type OptionType,
        Func<CommandProcessorContext, object, Task> TaskGeneratingFunction);
}
