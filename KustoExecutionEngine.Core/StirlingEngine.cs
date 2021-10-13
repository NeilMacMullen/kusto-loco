using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.Statements;
using System.Linq;

namespace KustoExecutionEngine.Core
{
    public class StirlingEngine
    {
        private readonly Stack<ExecutionContext> _executionContexts;

        public StirlingEngine()
        {
            _executionContexts = new Stack<ExecutionContext>();
        }

        private List<(string TableName, IEnumerable<string> ColumnNames, IEnumerable<IRow> Rows)> _globalTables = new();
        public void AddGlobalTable(string tableName, IEnumerable<string> columnNames, IEnumerable<IRow> rows)
        {
            _globalTables.Add((tableName, columnNames, rows));
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
                        "(", string.Join(",", tableDef.ColumnNames.Select(c => $"{c}: real")))
                ).ToArray());
            GlobalState globals = GlobalState.Default.WithDatabase(db);

            var globalObjects = _globalTables
                .Select(globalTable =>
                    new KeyValuePair<string, object?>(
                        globalTable.TableName,
                        new InMemoryTabularSource(globalTable.Rows.ToArray())))
                .ToArray();
            _executionContexts.Push(new ExecutionContext(null, globalObjects));

            var code = KustoCode.ParseAndAnalyze(query, globals);

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
        }

        internal void PushRowContext(IRow row)
        {
            var context = new ExecutionContext(_executionContexts.Peek(), row.ToArray());
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