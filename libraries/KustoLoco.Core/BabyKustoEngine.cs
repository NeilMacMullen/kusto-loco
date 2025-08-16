//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kusto.Language;
using Kusto.Language.Symbols;
using KustoLoco.Core.Console;
using KustoLoco.Core.Diagnostics;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Evaluation.BuiltIns;
using KustoLoco.Core.InternalRepresentation;
using KustoLoco.Core.Settings;
using NLog;

namespace KustoLoco.Core;

public readonly record struct FunctionInfo(string Name, bool Implemented);
public readonly record struct AggregateInfo(string Name, bool Implemented);

public class BabyKustoEngine
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private Dictionary<FunctionSymbol, ScalarFunctionInfo> _additionalfuncs = new();
    private readonly IKustoConsole _console;
    private readonly KustoSettingsProvider _settings;

    public BabyKustoEngine(IKustoConsole console,KustoSettingsProvider settings)
    {
        _settings = settings;
        _console = console;
        _settings.Register(CoreSettings.DumpIr,CoreSettings.DumpParseTree);
    }

    public static BabyKustoEngine CreateForTest()
    {
        var settings = GetSettingsWithFullDebug();
        return new BabyKustoEngine(new SystemConsole(), settings);
    }

    public static KustoSettingsProvider GetSettingsWithFullDebug()
    {
        var settings = new KustoSettingsProvider();
        settings.Set(CoreSettings.DumpIr.Name, "true");
        settings.Set(CoreSettings.DumpParseTree.Name, "true");
        settings.Set("kusto.stacktrace", "true");
        return settings;
    }

    public void AddAdditionalFunctions(Dictionary<FunctionSymbol, ScalarFunctionInfo> funcs)
    {
        _additionalfuncs = funcs;
    }

    public IReadOnlyCollection<AggregateInfo> GetImplementedAggregateList()
    {
        var aggregates = BuiltInAggregates.Aggregates.Select(a => new AggregateInfo(a.Key.Name, true))
            .ToArray();
        var notImplemented =
            GlobalState.Default.Aggregates.Except(BuiltInAggregates.Aggregates.Keys)
                .Select(f => new AggregateInfo(f.Name, false))
                .ToArray();
        return aggregates.Concat(notImplemented).ToArray();
    }


    public IReadOnlyCollection<FunctionInfo> GetImplementedFunctionList()
    {
        var funcs = BuiltInScalarFunctions.Functions.Concat(CustomFunctions.functions)
            .Select(f => new FunctionInfo(f.Key.Name, true))
            .ToArray();
        var notImplemented =
            GlobalState.Default.Functions.Except(BuiltInScalarFunctions.Functions.Keys)
                .Select(f => new FunctionInfo(f.Name, false))
                .ToArray();
        return funcs.Concat(notImplemented)
            .OrderBy(f => f.Name).ToArray();
    }

    public EvaluationResult Evaluate(
        IReadOnlyCollection<ITableSource> tables,
        string query)
    {
        var dumpKustoTree = _settings.GetBool(CoreSettings.DumpParseTree);
        var dumpIRTree = _settings.GetBool(CoreSettings.DumpIr);

        Logger.Trace("Evaluate called");
        //combine all available functions
        var allFuncs = BuiltInScalarFunctions.Functions.Concat(CustomFunctions.functions)
            .Concat(_additionalfuncs)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
        //some functions are implicitly implemented so use the existing default
        //set as a baseline
        var allSupported = GlobalState.Default.Functions.Concat(allFuncs.Keys)
            .Distinct().ToArray();

        var state = GlobalState.Default
            .WithFunctions(allSupported);

        var db = new DatabaseSymbol("tables", tables.Select(table => table.Type).Cast<Symbol>().ToArray());

        var globals = state.WithDatabase(db);

        var code = KustoCode.ParseAndAnalyze(query, globals);

        var visualizer = new IrNodeVisualizer(_console);
        visualizer.DumpKustoTree(code, dumpKustoTree);

        var diagnostics = code.GetDiagnostics();
        if (diagnostics.Count > 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Query error(s):");
            foreach (var diag in diagnostics)
            {
                sb.AppendLine("---");
                sb.AppendLine($"{diag.Message}");
                sb.AppendLine();
                sb.AppendLine(HighlightQuery(diag));
                sb.AppendLine();
            }
            
            throw new InvalidOperationException(
               sb.ToString());
        }
        string HighlightQuery(Diagnostic diag)
        {
            if (diag.End > query.Length)
                return query;
            if (diag.Length==0)
                return query.Substring(0, diag.Start) +
                       "<<<" + query.Substring(diag.End);
            return query.Substring(0, diag.Start) +
                   ">>>" + query.Substring(diag.Start, diag.Length) +
                   "<<<" + query.Substring(diag.End);
        }


        Logger.Trace("visiting with IRTranslator...");
        var irVisitor = new IRTranslator(allFuncs);

        var ir = code.Syntax.Accept(irVisitor);

        visualizer.DumpIRTree(ir, dumpIRTree);

        Logger.Trace("Adding tables to scope...");
        var scope = new LocalScope();
        foreach (var table in tables) scope.AddSymbol(table.Type, TabularResult.CreateUnvisualized(table));

        Logger.Trace("Evaluating in scope...");
        var result = BabyKustoEvaluator.Evaluate(ir, scope);
        return result;
    }

    private static class CoreSettings
    {
        private const string prefix = "core";

        public static readonly KustoSettingDefinition DumpIr = new(
            Setting("dumpir"), "dumps the internal representation",
            "false",
            nameof(Boolean));

        public static readonly KustoSettingDefinition DumpParseTree = new(Setting("dumpkl"),
            "dumps the internal kusto parse tree", "false", nameof(Boolean));

        private static string Setting(string setting)
        {
            return $"{prefix}.{setting}";
        }
    }
}
