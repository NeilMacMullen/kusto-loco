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

public class BabyKustoEngine
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly List<ITableSource> _globalTables = new();

    static BabyKustoEngine()
    {
        //ensure added functions are registered before engine is set up
        BuiltInScalarFunctions.Initialize();
    }

    public static GlobalState GlobalStateInstance { get; set; } = GlobalState.Default;

    public void AddGlobalTable(ITableSource table)
    {
        Logger.Trace($"Adding table  {table.Type.Name} ");
        if (string.IsNullOrEmpty(table.Type.Name))
        {
            throw new ArgumentNullException($"{nameof(table)}.{nameof(table.Type)}.{nameof(table.Type.Name)}");
        }

        _globalTables.Add(table);
    }

    public EvaluationResult Evaluate(string query, bool dumpKustoTree = false, bool dumpIRTree = false)
    {
        // TODO: davidni: Set up global state somehwere proper where it would be done just once

        Logger.Trace("Evaluate called");
        var db = new DatabaseSymbol(
            "MyDb",
            _globalTables.Select(table => table.Type).ToArray());
        var globals = GlobalStateInstance.WithDatabase(db);

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
        var irVisitor = new IRTranslator();

        var ir = code.Syntax.Accept(irVisitor);

        if (dumpIRTree)
        {
            Console.WriteLine("Internal representation:");
            DumpIRTree(ir);
            Console.WriteLine();
        }

        Logger.Trace("Adding tables to scope...");
        var scope = new LocalScope();
        foreach (var table in _globalTables)
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