using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.Statements;

namespace KustoExecutionEngine.Core
{
    public class StirlingEngine
    {
        private GlobalState _globals;

        public StirlingEngine()
        {
            var db = new DatabaseSymbol(
                    "MyDb",
                    new[]
                    {
                        new TableSymbol("MyTable", "(a: real, b: real)"),
                    });
            _globals = GlobalState.Default.WithDatabase(db);
        }

        public object Evaluate(string query)
        {
            var code = KustoCode.ParseAndAnalyze(query, _globals);

            var firstExpressionStatement = code.Syntax.GetFirstDescendant<ExpressionStatement>();
            var sterlingStatement = StirlingStatement.Build(this, firstExpressionStatement);
            return sterlingStatement.Execute();
        }
    }
}