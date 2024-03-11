// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Evaluation.BuiltIns;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using NLog;

namespace BabyKusto.Core;

public readonly record struct FunctionInfo(string Name, bool Implemented);

public class BabyKustoEngine
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    private Dictionary<FunctionSymbol, ScalarFunctionInfo> _additionalfuncs = new();
  
    public void AddAdditionalFunctions(Dictionary<FunctionSymbol, ScalarFunctionInfo> funcs)
    {
        _additionalfuncs = funcs;
    }

    public IReadOnlyCollection<FunctionInfo> GetImplementedList()
    {
        var funcs = BuiltInScalarFunctions.functions.Concat(CustomFunctions.functions)
            .Select(f => new FunctionInfo(f.Key.Name, true))
            .ToArray();
        var notImplemented =
            GlobalState.Default.Functions.Except(BuiltInScalarFunctions.functions.Keys)
                .Select(f => new FunctionInfo(f.Name, false))
                .ToArray();
        return funcs.Concat(notImplemented)
            .OrderBy(f => f.Name).ToArray();
    }

    public EvaluationResult Evaluate(
        IReadOnlyCollection<ITableSource> tables,
        string query, bool dumpKustoTree = false, bool dumpIRTree = false)
    {
        Logger.Trace("Evaluate called");
        //combine all available functions
        var allFuncs = BuiltInScalarFunctions.functions.Concat(CustomFunctions.functions)
            .Concat(_additionalfuncs)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
        //some functions are implicitly implemented so use the existing default
        //set as a baseline
        var allSupported = GlobalState.Default.Functions.Concat(allFuncs.Keys)
            .Distinct().ToArray();

        var state = GlobalState.Default
            .WithFunctions(allSupported);

        var db = new DatabaseSymbol("tables", tables.Select(table => table.Type).ToArray());

        var globals = state.WithDatabase(db);

        var code = KustoCode.ParseAndAnalyze(query, globals);
        if (dumpKustoTree)
        {
            Logger.Debug("Kusto tree:");
            var tree = DumpKustoTree(code);
            Logger.Debug(tree);
        }

        var diagnostics = code.GetDiagnostics();
        if (diagnostics.Count > 0)
        {
            foreach (var diag in diagnostics)
            {
                Logger.Warn($"Kusto diagnostics: {diag.Severity} {diag.Code} {diag.Message} {diag.Description}");
            }

            throw new InvalidOperationException(
                $"Query is malformed.\r\n{string.Join("\r\n", diagnostics.Select(diag => $"[{diag.Start}] {diag.Severity} {diag.Code} {diag.Message} {diag.Description}"))}");
        }

        Logger.Trace("visiting with IRTranslator...");
        var irVisitor = new IRTranslator(allFuncs);

        var ir = code.Syntax.Accept(irVisitor);

        if (dumpIRTree)
        {
            Console.WriteLine("Internal representation:");
            DumpIRTree(ir);
            Console.WriteLine();
        }

        Logger.Trace("Adding tables to scope...");
        var scope = new LocalScope();
        foreach (var table in tables)
        {
            scope.AddSymbol(table.Type, TabularResult.CreateUnvisualized(table));
        }

        Logger.Trace("Evaluating in scope...");
        var result = BabyKustoEvaluator.Evaluate(ir, scope);
        return result;

        static string DumpKustoTree(KustoCode code)
        {
            var sb = new StringBuilder();
            var indent = 0;
            SyntaxElement.WalkNodes(
                code.Syntax,
                fnBefore: node =>
                {
                    sb.Append(new string(' ', indent));
                    sb.AppendLine(
                        $"{node.Kind}: {node.ToString(IncludeTrivia.SingleLine)}: {SchemaDisplay.GetText((node as Expression)?.ResultType)}");
                    indent++;
                },
                fnAfter: node => { indent--; });

            sb.AppendLine();
            sb.AppendLine();
            return sb.ToString();
        }

        static void DumpIRTree(IRNode node)
        {
            DumpTreeInternal(node, "");

            Console.WriteLine();
            Console.WriteLine();

            static void DumpTreeInternal(IRNode node, string indent, bool isLast = true)
            {
                var oldColor = Console.ForegroundColor;
                try
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                    Console.Write(indent);
                    Console.Write(isLast ? " └─" : " ├─");

                    Console.ForegroundColor = node switch
                    {
                        IRListNode => ConsoleColor.DarkGray,
                        IRStatementNode => ConsoleColor.White,
                        IRQueryOperatorNode => ConsoleColor.DarkBlue,
                        IRLiteralExpressionNode => ConsoleColor.Magenta,
                        IRNameReferenceNode => ConsoleColor.Green,
                        IRExpressionNode => ConsoleColor.Cyan,
                        _ => ConsoleColor.Gray,
                    };

                    Console.WriteLine(node);
                }
                finally
                {
                    Console.ForegroundColor = oldColor;
                }

                indent += isLast ? "   " : " | ";

                for (var i = 0; i < node.ChildCount; i++)
                {
                    var child = node.GetChild(i);
                    DumpTreeInternal(child, indent, i == node.ChildCount - 1);
                }
            }
        }
    }
}