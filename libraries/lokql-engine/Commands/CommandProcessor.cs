using System.Collections.Immutable;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Reflection;
using CommandLine;
using CsvHelper;

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
        var cp= new CommandProcessor()
                .WithAdditionalCommand<LoadCommand.Options>(LoadCommand.RunAsync)
                .WithAdditionalCommand<SaveCommand.Options>(SaveCommand.RunAsync)
                .WithAdditionalCommand<SetCommand.Options>(SetCommand.RunAsync)
                .WithAdditionalCommand<SettingsCommand.Options>(SettingsCommand.RunAsync)
                .WithAdditionalCommand<KnownSettingsCommand.Options>(KnownSettingsCommand.RunAsync)
                .WithAdditionalCommand<PushCommand.Options>(PushCommand.RunAsync)
                .WithAdditionalCommand<PullCommand.Options>(PullCommand.RunAsync)
                .WithAdditionalCommand<ResultsCommand.Options>(ResultsCommand.RunAsync)
                .WithAdditionalCommand<CsvDataCommand.Options>(CsvDataCommand.RunAsync)
                .WithAdditionalCommand<ListTablesCommand.Options>(ListTablesCommand.RunAsync)
                .WithAdditionalCommand<MaterializeCommand.Options>(MaterializeCommand.RunAsync)
                .WithAdditionalCommand<SynTableCommand.Options>(SynTableCommand.RunAsync)
                .WithAdditionalCommand<FileFormatsCommand.Options>(FileFormatsCommand.RunAsync)
                .WithAdditionalCommand<AppInsightsCommand.Options>(AppInsightsCommand.RunAsync)
                .WithAdditionalCommand<StartReportCommand.Options>(StartReportCommand.RunAsync)
                .WithAdditionalCommand<AddToReportCommand.Options>(AddToReportCommand.RunAsync)
                .WithAdditionalCommand<FinishReportCommand.Options>(FinishReportCommand.RunAsync)
                .WithAdditionalCommand<EchoCommand.Options>(EchoCommand.RunAsync)
                .WithAdditionalCommand<PivotCommand.Options>(PivotCommand.RunAsync)
                .WithAdditionalCommand<SetScalarCommand.Options>(SetScalarCommand.RunAsync)


                .WithAdditionalCommand<RenderCommand.Options>(RenderCommand.RunAsync)
                .WithAdditionalCommand<ExitCommand.Options>(ExitCommand.RunAsync)
                .WithAdditionalCommand<FormatCommand.Options>(FormatCommand.RunAsync)
                .WithAdditionalCommand<RunScriptCommand.Options>(RunScriptCommand.RunAsync)
                .WithAdditionalCommand<SaveQueryCommand.Options>(SaveQueryCommand.RunAsync)
                .WithAdditionalCommand<QueryCommand.Options>(QueryCommand.RunAsync)
                .WithAdditionalCommand<DefineMacroCommand.Options>(DefineMacroCommand.RunAsync)
                .WithAdditionalCommand<RunMacroCommand.Options>(RunMacroCommand.RunAsync)
                .WithAdditionalCommand<SleepCommand.Options>(SleepCommand.RunAsync)
            ;

        cp.AddAdditionalCommandSchema(AppInsightsCommand.SchemaCsv);
        return cp;
    }

    public void AddAdditionalCommandSchema(string schemaCsv)
    {
            var stream = new StringReader(schemaCsv);
            using var csv = new CsvReader(stream, CultureInfo.InvariantCulture);
            var schema= csv.GetRecords<SchemaLine>().ToArray();
            _registeredSchema = _registeredSchema.Concat(schema).ToArray();
    }


    private SchemaLine[] _registeredSchema = [];
    public CommandProcessor WithAdditionalCommand<T>(Func<CommandProcessorContext, T, Task> registration)
    {
        _registrations = _registrations.Add(
            new RegisteredCommand(typeof(T), (exp, o) => registration(exp, (T)o)));
        return this;
    }

    public SchemaLine [] GetRegisteredSchema()
    {
        return _registeredSchema;
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
