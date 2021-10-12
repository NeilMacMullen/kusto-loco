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

        public object Evaluate(string query)
        {
            var code = KustoCode.ParseAndAnalyze(query, _globals);

            var firstExpressionStatement = code.Syntax.GetFirstDescendant<ExpressionStatement>();
            var sterlingStatement = StirlingStatement.Build(this, firstExpressionStatement);
            return sterlingStatement.Execute();
        }

        internal void PushRowContext(IRow row)
        {
            var context = new ExecutionContext(_executionContexts.Peek(), row.ToArray());
            _executionContexts.Push(context);
        }

        internal void LeaveExecutionContext()
        {
            _executionContexts.Pop();
        }
    }
}