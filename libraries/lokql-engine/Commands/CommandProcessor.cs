using System.Collections.Immutable;
using System.CommandLine.Parsing;
using System.Reflection;
using CommandLine;

namespace Lokql.Engine.Commands;

public readonly record struct CommandProcessorContext(InteractiveTableExplorer Explorer, BlockSequence Sequence);

public class CommandProcessor
{
    private ImmutableDictionary<Type, Func<CommandProcessorContext, object, Task>> _registrations
        = ImmutableDictionary<Type, Func<CommandProcessorContext, object, Task>>.Empty;

    private CommandProcessor()
    {
    }

    public static CommandProcessor Default()
    {
        return new CommandProcessor()
            .WithAdditionalCommand<QuickCsvCommand.Options>(QuickCsvCommand.RunAsync)
            .WithAdditionalCommand<AllTablesCommand.Options>(AllTablesCommand.Run)
            .WithAdditionalCommand<LoadCommand.Options>(LoadCommand.RunAsync)
            .WithAdditionalCommand<PivotCommand.Options>(PivotCommand.Run)
            .WithAdditionalCommand<MaterializeCommand.Options>(MaterializeCommand.Run)
            .WithAdditionalCommand<RenderCommand.Options>(RenderCommand.Run)
            .WithAdditionalCommand<ExitCommand.Options>(ExitCommand.Run)
            .WithAdditionalCommand<FormatCommand.Options>(FormatCommand.Run)
            .WithAdditionalCommand<SynTableCommand.Options>(SynTableCommand.Run)
            .WithAdditionalCommand<RunScriptCommand.Options>(RunScriptCommand.RunAsync)
            .WithAdditionalCommand<SaveQueryCommand.Options>(SaveQueryCommand.RunAsync)
            .WithAdditionalCommand<SaveCommand.Options>(SaveCommand.RunAsync)
            .WithAdditionalCommand<QueryCommand.Options>(QueryCommand.RunAsync)
            .WithAdditionalCommand<ShowCommand.Options>(ShowCommand.RunAsync)
            .WithAdditionalCommand<FileFormatsCommand.Options>(FileFormatsCommand.RunAsync)
            .WithAdditionalCommand<SetCommand.Options>(SetCommand.RunAsync)
            .WithAdditionalCommand<ListSettingsCommand.Options>(ListSettingsCommand.RunAsync)
            .WithAdditionalCommand<ListSettingDefinitionsCommand.Options>(ListSettingDefinitionsCommand.RunAsync)
            .WithAdditionalCommand<AppInsightsCommand.Options>(AppInsightsCommand.RunAsync)
            .WithAdditionalCommand<DefineMacroCommand.Options>(DefineMacroCommand.RunAsync)
            .WithAdditionalCommand<RunMacroCommand.Options>(RunMacroCommand.RunAsync)
            .WithAdditionalCommand<StartReportCommand.Options>(StartReportCommand.Run)
            .WithAdditionalCommand<RenderToReportCommand.Options>(RenderToReportCommand.Run)
            .WithAdditionalCommand<RenderTableToReportCommand.Options>(RenderTableToReportCommand.Run)
            .WithAdditionalCommand<SleepCommand.Options>(SleepCommand.Run)
            .WithAdditionalCommand<EndReportCommand.Options>(EndReportCommand.Run)
            .WithAdditionalCommand<RenderTableToText.Options>(RenderTableToText.Run)
            .WithAdditionalCommand<PushCommand.Options>(PushCommand.RunAsync)
            .WithAdditionalCommand<PullCommand.Options>(PullCommand.RunAsync);
    }

    public CommandProcessor WithAdditionalCommand<T>(Func<CommandProcessorContext, T, Task> registration)
    {
        _registrations = _registrations.Add(typeof(T), (exp, o) => registration(exp, (T)o));
        return this;
    }

    public async Task RunInternalCommand(InteractiveTableExplorer exp, string currentLine, BlockSequence sequence)
    {
        var splitter = CommandLineStringSplitter.Instance;
        var tokens = splitter.Split(currentLine).ToArray();

        if (!tokens.Any())
            return;


        var textWriter = new StringWriter();

        var table = _registrations.Keys.ToArray();

        var result = StandardParsers.CreateWithHelpWriter(textWriter)
            .ParseArguments(tokens, table);

        var context = new CommandProcessorContext(exp, sequence);
        foreach (var registration in _registrations)
        {
            async Task Func(object o)
            {
                await registration.Value(context, o);
            }

            await result.TryAsync(registration.Key, Func);
        }

        exp.Info(textWriter.ToString());
    }

    public Dictionary<string, string> GetVerbs()
    {
        return _registrations.Keys
            .SelectMany(t => t.GetTypeInfo().GetCustomAttributes(typeof(VerbAttribute), true))
            .OfType<VerbAttribute>()
            .ToDictionary(a => a.Name, a => a.HelpText);
    }
}
