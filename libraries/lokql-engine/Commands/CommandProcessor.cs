using System.Collections.Immutable;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Reflection;
using CommandLine;
using CsvHelper;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public readonly record struct CommandProcessorContext(InteractiveTableExplorer Explorer, BlockSequence Sequence);

public class CommandProcessor
{
    private SchemaLine[] _registeredSchema = [];

    private ImmutableList<RegisteredCommand> _registrations
        = [];

    private CommandProcessor()
    {
    }

    public static CommandProcessor Default()
    {
        var cp = new CommandProcessor()
                .WithAdditionalCommand<LoadCommand.Options>(LoadCommand.RunAsync)
                .WithAdditionalCommand<SaveCommand.Options>(SaveCommand.RunAsync)
                .WithAdditionalCommand<SetCommand.Options>(SetCommand.RunAsync)
                .WithAdditionalCommand<SettingsCommand.Options>(SettingsCommand.RunAsync)
                .WithAdditionalCommand<KnownSettingsCommand.Options>(KnownSettingsCommand.RunAsync)
                .WithAdditionalCommand<PushCommand.Options>(PushCommand.RunAsync)
                .WithAdditionalCommand<PullCommand.Options>(PullCommand.RunAsync)
                .WithAdditionalCommand<ResultsCommand.Options>(ResultsCommand.RunAsync)
                .WithAdditionalCommand<AddTableCommand.Options>(AddTableCommand.RunAsync)
                .WithAdditionalCommand<ListTablesCommand.Options>(ListTablesCommand.RunAsync)
                .WithAdditionalCommand<MaterializeCommand.Options>(MaterializeCommand.RunAsync)
                .WithAdditionalCommand<SynTableCommand.Options>(SynTableCommand.RunAsync)
                .WithAdditionalCommand<FileFormatsCommand.Options>(FileFormatsCommand.RunAsync)
                .WithAdditionalCommand<AppInsightsCommand.Options>(AppInsightsCommand.RunAsync)
                .WithAdditionalCommand<AdxCommand.Options>(AdxCommand.RunAsync)
                .WithAdditionalCommand<StartReportCommand.Options>(StartReportCommand.RunAsync)
                .WithAdditionalCommand<AddToReportCommand.Options>(AddToReportCommand.RunAsync)
                .WithAdditionalCommand<FinishReportCommand.Options>(FinishReportCommand.RunAsync)
                .WithAdditionalCommand<EchoCommand.Options>(EchoCommand.RunAsync)
                .WithAdditionalCommand<PivotColumnsToRowsCommand.Options>(PivotColumnsToRowsCommand.RunAsync)
                .WithAdditionalCommand<PivotRowsToColumnsCommand.Options>(PivotRowsToColumnsCommand.RunAsync)
                .WithAdditionalCommand<SetScalarCommand.Options>(SetScalarCommand.RunAsync)
                .WithAdditionalCommand<LoadExcel.Options>(LoadExcel.RunAsync)
                .WithAdditionalCommand<RenderCommand.Options>(RenderCommand.RunAsync)
                .WithAdditionalCommand<ExitCommand.Options>(ExitCommand.RunAsync)
                .WithAdditionalCommand<FormatCommand.Options>(FormatCommand.RunAsync)
                .WithAdditionalCommand<RunScriptCommand.Options>(RunScriptCommand.RunAsync)
                .WithAdditionalCommand<SaveQueryCommand.Options>(SaveQueryCommand.RunAsync)
                .WithAdditionalCommand<QueryCommand.Options>(QueryCommand.RunAsync)
                .WithAdditionalCommand<DefineMacroCommand.Options>(DefineMacroCommand.RunAsync)
                .WithAdditionalCommand<RunMacroCommand.Options>(RunMacroCommand.RunAsync)
                .WithAdditionalCommand<SleepCommand.Options>(SleepCommand.RunAsync)
                .WithAdditionalCommand<CopilotCommand.Options>(CopilotCommand.RunAsync)
            ;

        cp.AddAdditionalCommandSchema(AppInsightsCommand.SchemaCsv);
        return cp;
    }

    public void AddAdditionalCommandSchema(string schemaCsv)
    {
        var stream = new StringReader(schemaCsv);
        using var csv = new CsvReader(stream, CultureInfo.InvariantCulture);
        var schema = csv.GetRecords<SchemaLine>().ToArray();
        _registeredSchema = _registeredSchema.Concat(schema).ToArray();
    }

    public CommandProcessor WithAdditionalCommand<T>(Func<CommandProcessorContext, T, Task> registration)
    {
        _registrations = _registrations.Add(
            new RegisteredCommand(typeof(T), (exp, o) => registration(exp, (T)o)));
        return this;
    }

    public SchemaLine[] GetRegisteredSchema() => _registeredSchema;


    public async Task RunInternalCommand(InteractiveTableExplorer exp, string currentLine, BlockSequence sequence)
    {
        var splitter = CommandLineStringSplitter.Instance;
        var tokens = splitter.Split(currentLine).ToArray();

        if (!tokens.Any())
            return;


        var textWriter = new StringWriter();

        var typeTable = _registrations.Select(r => r.OptionType).ToArray();

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

        var error = textWriter.ToString();
        if (error.IsNotBlank())
            exp.Warn(error);
    }

    public IEnumerable<VerbEntry> GetVerbs()
    {
        var verbs = from type in _registrations.Select(x => x.OptionType)
            let attribute = type.GetTypeInfo().GetCustomAttribute<VerbAttribute>()
                            ?? throw new InvalidOperationException(
                                $"All registered command options should have a {nameof(VerbAttribute)}. {type.FullName ?? type.Name} does not.")
            let supportsFiles = type.IsAssignableTo(typeof(IFileCommandOption))
            select new VerbEntry(attribute.Name, attribute.HelpText, supportsFiles);

        return verbs.Append(CreateHelpEntry());

        static VerbEntry CreateHelpEntry()
        {
            // Help is a default command in CommandLineParser but we still need to provide metadata for it.
            const string helpText = @"Shows a list of available commands or help for a specific command
.help            for a summary of all commands
.help *command*  for details of a specific command";
            return new VerbEntry("help", helpText, false);
        }
    }

    private readonly record struct RegisteredCommand(
        Type OptionType,
        Func<CommandProcessorContext, object, Task> TaskGeneratingFunction);
}
