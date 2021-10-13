using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.Statements;
using System.Linq;

namespace KustoExecutionEngine.Core
{
    public class StirlingEngine
    {
        private readonly GlobalState _globals;
        private readonly Stack<ExecutionContext> _executionContexts;

        public StirlingEngine()
        {
            var db = new DatabaseSymbol(
                    "MyDb",
                    new[]
                    {
                        new TableSymbol("MyTable", "(a: real, b: real)"),
                    });
            _globals = GlobalState.Default.WithDatabase(db);

            _executionContexts = new Stack<ExecutionContext>();

            var myTable = new InMemoryTabularSource(
                new[]
                {
                    new Row(
                        new[]
                        {
                            new KeyValuePair<string, object?>("a", 1.0),
                            new KeyValuePair<string, object?>("b", 2.0),
                        }),
                    new Row(
                        new[]
                        {
                            new KeyValuePair<string, object?>("a", 1.5),
                            new KeyValuePair<string, object?>("b", 2.5),
                        }),
                });
            _executionContexts.Push(new ExecutionContext(null, new KeyValuePair<string, object?>("MyTable", myTable)));
        }

        internal ExecutionContext ExecutionContext => _executionContexts.Peek();

        public object? Evaluate(string query)
        {
            var code = KustoCode.ParseAndAnalyze(query, _globals);

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