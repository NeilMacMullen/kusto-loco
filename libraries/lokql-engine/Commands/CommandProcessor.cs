using System.Collections.Immutable;
using System.CommandLine.Parsing;
using CommandLine;

namespace Lokql.Engine.Commands;

public class CommandProcessor
{

    private CommandProcessor()
    {

    }

    public static CommandProcessor Default()
    {
        return new CommandProcessor()
            .WithAdditionalCommand<AllTablesCommand.Options>(AllTablesCommand.Run)
            .WithAdditionalCommand<LoadCommand.Options>(LoadCommand.RunAsync);
    }

    private ImmutableDictionary<Type, Func<InteractiveTableExplorer, object, Task>> _registrations=ImmutableDictionary<Type, Func<InteractiveTableExplorer, object, Task>>.Empty;
    public CommandProcessor WithAdditionalCommand<T>(Func <InteractiveTableExplorer,T,Task> registration)
    {
        _registrations = _registrations.Add(typeof(T), (exp,o) => registration(exp,(T) o));
        return this;
    }
    public async Task RunInternalCommand(InteractiveTableExplorer exp, string currentLine, BlockSequence sequence)
    {
        var splitter = CommandLineStringSplitter.Instance;
        var tokens = splitter.Split(currentLine).ToArray();

        if (!tokens.Any())
            return;


        var textWriter = new StringWriter();

        var table = new Type[]
        {
            typeof(ExitCommand.Options),
            typeof(SaveCommand.Options),
            typeof(RenderCommand.Options),
            typeof(FormatCommand.Options),
            typeof(RunScriptCommand.Options),
            typeof(QueryCommand.Options),
            typeof(SaveQueryCommand.Options),
            typeof(MaterializeCommand.Options),
            typeof(SynTableCommand.Options),
            typeof(ShowCommand.Options),
            typeof(FileFormatsCommand.Options),
            typeof(SetCommand.Options),
            typeof(ListSettingsCommand.Options),
            typeof(ListSettingDefinitionsCommand.Options),
            typeof(AppInsightsCommand.Options),
            typeof(PivotCommand.Options)
        }.Concat(_registrations.Keys)
        .ToArray();

        var result= await StandardParsers.CreateWithHelpWriter(textWriter)
                .ParseArguments(tokens,table)
                .WithParsed<PivotCommand.Options>(o => PivotCommand.Run(exp, o))
                .WithParsed<MaterializeCommand.Options>(o => MaterializeCommand.Run(exp, o))
                .WithParsed<RenderCommand.Options>(o => RenderCommand.Run(exp, o))
                .WithParsed<ExitCommand.Options>(o => ExitCommand.Run(exp, o))
                .WithParsed<FormatCommand.Options>(o => FormatCommand.Run(exp, o))
                .WithParsed<SynTableCommand.Options>(o => SynTableCommand.Run(exp, o))
                .WithParsedAsync<RunScriptCommand.Options>(o => RunScriptCommand.RunAsync(exp, o))
                .WithParsedAsync<SaveQueryCommand.Options>(o => SaveQueryCommand.RunAsync(exp, o))
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

        foreach (var registration in _registrations)
        {
            await result.TryAsync(registration.Key, o => registration.Value(exp, o));
        }
        exp.Info(textWriter.ToString());
    }
}
