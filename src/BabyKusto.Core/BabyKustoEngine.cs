// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;

namespace BabyKusto.Core
{
    public class BabyKustoEngine
    {
        private readonly List<(string TableName, ITableSource Source)> _globalTables = new();

        public void AddGlobalTable(string tableName, ITableSource source)
        {
            _globalTables.Add((tableName, source));
        }

        public EvaluationResult? Evaluate(string query, bool dumpKustoTree = false, bool dumpIRTree = false)
        {
            // TODO: davidni: Set up global state somehwere proper where it would be done just once
            var db = new DatabaseSymbol(
                "MyDb",
                _globalTables.Select(table => table.Source.Type).ToArray());
            GlobalState globals = GlobalState.Default.WithDatabase(db);

            var code = KustoCode.ParseAndAnalyze(query, globals);
            if (dumpKustoTree)
            {
                Console.WriteLine("Kusto tree:");
                DumpKustoTree(code);
                Console.WriteLine();
            }

            var diagnostics = code.GetDiagnostics();
            if (diagnostics.Count > 0)
            {
                foreach (var diag in diagnostics)
                {
                    Console.WriteLine($"Kusto diagnostics: {diag.Severity} {diag.Code} {diag.Message} {diag.Description}");
                }

                throw new InvalidOperationException($"Query is malformed.\r\n{string.Join("\r\n", diagnostics.Select(diag => $"{diag.Severity} {diag.Code} {diag.Message} {diag.Description}"))}");
            }

            var irVisitor = new IRTranslator();
            var ir = code.Syntax.Accept(irVisitor);

            if (dumpIRTree)
            {
                Console.WriteLine("Internal representation:");
                DumpIRTree(ir);
                Console.WriteLine();
            }

            var scope = new LocalScope();
            for (int i = 0; i < _globalTables.Count; i++)
            {
                scope.AddSymbol(db.Tables[i], new TabularResult(_globalTables[i].Source));
            }

            var result = BabyKustoEvaluator.Evaluate(ir, scope);
            return result;

            static void DumpKustoTree(KustoCode code)
            {
                int indent = 0;
                SyntaxElement.WalkNodes(
                    code.Syntax,
                    fnBefore: node =>
                    {
                        Console.Write(new string(' ', indent));
                        Console.WriteLine($"{node.Kind}: {node.ToString(IncludeTrivia.SingleLine)}: {(node as Expression)?.ResultType?.Display}");
                        indent++;
                    },
                    fnAfter: node =>
                    {
                        indent--;
                    });

                Console.WriteLine();
                Console.WriteLine();
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

                    for (int i = 0; i < node.ChildCount; i++)
                    {
                        var child = node.GetChild(i);
                        DumpTreeInternal(child, indent, i == node.ChildCount - 1);
                    }
                }
            }
        }
    }
}