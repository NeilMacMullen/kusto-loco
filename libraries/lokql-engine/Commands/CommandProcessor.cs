using System.Collections.Immutable;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Reflection;
using CommandLine;
using CsvHelper;
using KustoLoco.PluginSupport;
using NLog.LayoutRenderers;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public class CommandProcessor :ICommandProcessor
{
    private SchemaLine[] _registeredSchema = [];

    private ImmutableList<RegisteredCommand> _registrations
        = [];

    private CommandProcessor()
    {
    }

    public static CommandProcessor Default()
    {
        var cp = (CommandProcessor) new CommandProcessor()
                 //loader not exposed 
                .LegacyWithAdditionalCommand<LoadCommand.Options>(LoadCommand.RunAsync) 
                .LegacyWithAdditionalCommand<SaveCommand.Options>(SaveCommand.RunAsync)
                 //results history not exposed
                .LegacyWithAdditionalCommand<ResultsCommand.Options>(ResultsCommand.RunAsync)
                .LegacyWithAdditionalCommand<FileFormatsCommand.Options>(FileFormatsCommand.RunAsync)

                .LegacyWithAdditionalCommand<StartReportCommand.Options>(StartReportCommand.RunAsync)
                .LegacyWithAdditionalCommand<AddToReportCommand.Options>(AddToReportCommand.RunAsync)
                .LegacyWithAdditionalCommand<FinishReportCommand.Options>(FinishReportCommand.RunAsync)
               
                .LegacyWithAdditionalCommand<LoadExcel.Options>(LoadExcel.RunAsync)
                .LegacyWithAdditionalCommand<RenderCommand.Options>(RenderCommand.RunAsync)
                .LegacyWithAdditionalCommand<FormatCommand.Options>(FormatCommand.RunAsync)
                .LegacyWithAdditionalCommand<RunScriptCommand.Options>(RunScriptCommand.RunAsync)
                .LegacyWithAdditionalCommand<SaveQueryCommand.Options>(SaveQueryCommand.RunAsync)
                .LegacyWithAdditionalCommand<QueryCommand.Options>(QueryCommand.RunAsync)
                .LegacyWithAdditionalCommand<DefineMacroCommand.Options>(DefineMacroCommand.RunAsync)
                .LegacyWithAdditionalCommand<RunMacroCommand.Options>(RunMacroCommand.RunAsync)
                .WithAdditionalCommand<SleepCommand.Options>(SleepCommand.RunAsync)
                .WithAdditionalCommand<CopilotCommand.Options>(CopilotCommand.RunAsync)
                .WithAdditionalCommand<GetEventLogCommand.Options>(GetEventLogCommand.RunAsync)
                .WithAdditionalCommand<AdxCommand.Options>(AdxCommand.RunAsync)
                .WithAdditionalCommand<PivotColumnsToRowsCommand.Options>(PivotColumnsToRowsCommand.RunAsync)
                .WithAdditionalCommand<SetCommand.Options>(SetCommand.RunAsync)
                .WithAdditionalCommand<SettingsCommand.Options>(SettingsCommand.RunAsync)
                .WithAdditionalCommand<KnownSettingsCommand.Options>(KnownSettingsCommand.RunAsync)
                .WithAdditionalCommand<PushCommand.Options>(PushCommand.RunAsync)
                .WithAdditionalCommand<PullCommand.Options>(PullCommand.RunAsync)
                .WithAdditionalCommand<DropTableCommand.Options>(DropTableCommand.RunAsync)
                .WithAdditionalCommand<RenameTableCommand.Options>(RenameTableCommand.RunAsync)
                .WithAdditionalCommand<GetClipboardCommand.Options>(GetClipboardCommand.RunAsync)
                 .WithAdditionalCommand<AddTableCommand.Options>(AddTableCommand.RunAsync)
                .WithAdditionalCommand<ListTablesCommand.Options>(ListTablesCommand.RunAsync)
                .WithAdditionalCommand<MaterializeCommand.Options>(MaterializeCommand.RunAsync)
                .WithAdditionalCommand<SynTableCommand.Options>(SynTableCommand.RunAsync)
                .WithAdditionalCommand<AppInsightsCommand.Options>(AppInsightsCommand.RunAsync)
                .WithAdditionalCommand<EchoCommand.Options>(EchoCommand.RunAsync)
                .WithAdditionalCommand<PivotRowsToColumnsCommand.Options>(PivotRowsToColumnsCommand.RunAsync)
                .WithAdditionalCommand<SetScalarCommand.Options>(SetScalarCommand.RunAsync)
                .WithAdditionalCommand<ExitCommand.Options>(ExitCommand.RunAsync)



            ;

        AppInsightsCommand.RegisterSchema(cp);
        return cp;
    }

    
    public void RegisterSchema(string command,string schemaText)
    {
        command = command.Trim();
        if (!command.StartsWith("."))
            command = $".{command}";
        var schema = schemaText.Tokenize("\r\n")
            .Select(l=>l.Tokenize(","))
            .Select(pair=>new SchemaLine(command, pair[0], pair[1]))
            .ToArray();
        _registeredSchema = _registeredSchema.Concat(schema).ToArray();
    }

    public CommandProcessor LegacyWithAdditionalCommand<T>(Func<CommandContext, T, Task> registration)
    {
        _registrations = _registrations.Add(
            new RegisteredCommand(typeof(T), (exp, o) => registration((CommandContext) exp, (T)o)));
        return this;
    }


    public ICommandProcessor WithAdditionalCommand<T>(Func<ICommandContext, T, Task> registration)
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

        var context = new CommandContext(exp, sequence);
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

    public IEnumerable<VerbEntry> GetVerbs(ITableAdaptor loader)
    {
        var extensions = loader.GetSupportedAdaptors().SelectMany(x => x.Extensions).ToList();
        var verbs = _registrations.Select(x =>
            {
                var attribute = x.OptionType.GetTypeInfo()
                    .GetCustomAttribute<VerbAttribute>()
                                ?? throw new InvalidOperationException($"All registered command options should have a {nameof(VerbAttribute)}. {x.OptionType.FullName ?? x.OptionType.Name} does not.");

                // for now we are only expecting one field, if we need to support multiple, we can change this
                var fileAttribute = x.OptionType
                    .GetProperties()
                    .Select(p => p.GetCustomAttribute<FileOptionsAttribute>())
                    .OfType<FileOptionsAttribute>()
                    .SingleOrDefault();

                var supportsFiles = false;

                IReadOnlyList<string> supportedExtensions = [];
                if (fileAttribute is not null)
                {
                    supportsFiles = true;
                    supportedExtensions = fileAttribute.Extensions;
                    if (fileAttribute.IncludeStandardFormatterExtensions)
                    {
                        supportedExtensions = supportedExtensions.Concat(extensions).ToList();
                    }
                }

                return new VerbEntry(attribute.Name, attribute.HelpText, supportsFiles, supportedExtensions);
            }
        );

        return verbs.Append(CreateHelpEntry());

        static VerbEntry CreateHelpEntry()
        {
            // Help is a default command in CommandLineParser but we still need to provide metadata for it.
            const string helpText = @"Shows a list of available commands or help for a specific command
.help            for a summary of all commands
.help *command*  for details of a specific command";
            return new VerbEntry("help", helpText, false, []);
        }
    }

    private readonly record struct RegisteredCommand(
        Type OptionType,
        Func<ICommandContext, object, Task> TaskGeneratingFunction);
}
