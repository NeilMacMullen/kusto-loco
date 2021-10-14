using System;
using System.Collections.Generic;
using System.Linq;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.DataSource;
using KustoExecutionEngine.Core.Statements;

namespace KustoExecutionEngine.Core
{
    public class StirlingEngine
    {
        private readonly List<(string TableName, TableSchema tableSchema, ITableChunk tableChunk)> _globalTables = new();
        private readonly Stack<ExecutionContext> _executionContexts;

        public StirlingEngine()
        {
            _executionContexts = new Stack<ExecutionContext>();
        }

        public void AddGlobalTable(string tableName, TableSchema tableSchema, ITableChunk tableChunk)
        {
            _globalTables.Add((tableName, tableSchema, tableChunk));
        }

        internal ExecutionContext ExecutionContext => _executionContexts.Peek();

        public object? Evaluate(string query)
        {
            // TODO: davidni: Set up global state somehwere proper where it would be done just once
            var db = new DatabaseSymbol(
                "MyDb",
                _globalTables.Select(
                    tableDef => new TableSymbol(
                        tableDef.TableName,
                        "(" + string.Join(",", tableDef.tableSchema.ColumnDefinitions.Select(c => $"{c.ColumnName}: {c.ValueKind.ToString().ToLower()}")) + ")")
                ).ToArray());
            GlobalState globals = GlobalState.Default.WithDatabase(db);

            var globalObjects = _globalTables
                .Select(globalTable =>
                    new KeyValuePair<string, object?>(
                        globalTable.TableName,
                        new InMemoryTabularSourceV2(globalTable.tableSchema, Enumerable.Repeat(globalTable.tableChunk, 1))))
                .ToArray();
            _executionContexts.Push(new ExecutionContext(null, globalObjects));

            var code = KustoCode.ParseAndAnalyze(query, globals);
            DumpTree(code);

            var diagnostics = code.GetDiagnostics();
            if (diagnostics.Count > 0)
            {
                foreach (var diag in diagnostics)
                {
                    Console.WriteLine($"Kusto diagnostics: {diag}");
                }

                throw new InvalidOperationException("Query is malformed.");
            }

            if (code.Syntax is not QueryBlock queryBlock)
            {
                throw new InvalidOperationException($"Only QueryBlocks are supported as top level syntax elements, found {TypeNameHelper.GetTypeDisplayName(code.Syntax)}");
            }

            object? lastResult = null;
            foreach (var statementEntry in queryBlock.Statements)
            {
                var statement = statementEntry.Element;
                var sterlingStatement = StirlingStatement.Build(this, statement);
                lastResult = sterlingStatement.Execute();
            }

            return lastResult;

            static void DumpTree(KustoCode code)
            {
                int indent = 0;
                SyntaxElement.WalkNodes(
                    code.Syntax,
                    fnBefore: node =>
                    {
                        Console.Write(new string(' ', indent));
                        Console.WriteLine($"{node.Kind} ({TypeNameHelper.GetTypeDisplayName(node.GetType())}): {node.ToString(IncludeTrivia.SingleLine)}");
                        indent++;
                    },
                    fnAfter: node =>
                    {
                        indent--;
                    });

                Console.WriteLine();
                Console.WriteLine();
            }
        }

        internal void PushRowContext(IRow row)
        {
            var bindings = new KeyValuePair<string, object?>[row.Schema.ColumnDefinitions.Count];
            for (int i = 0; i < row.Schema.ColumnDefinitions.Count; i++)
            {
                var columnDef = row.Schema.ColumnDefinitions[i];
                bindings[i] = new KeyValuePair<string, object?>(columnDef.ColumnName, row.Values[i]);
            }

            var context = new ExecutionContext(_executionContexts.Peek(), bindings);
            _executionContexts.Push(context);
        }

        internal void PushDeclarationsContext(string name, object? value)
        {
            var context = new ExecutionContext(_executionContexts.Peek(), new KeyValuePair<string, object?>(name, value));
            _executionContexts.Push(context);
        }

        internal void LeaveExecutionContext()
        {
            _executionContexts.Pop();
        }
    }
}