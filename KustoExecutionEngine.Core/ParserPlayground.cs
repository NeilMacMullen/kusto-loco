using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core
{
    public class ParserPlayground
    {
        private GlobalState _globals;

        public ParserPlayground()
        {
            var db = new DatabaseSymbol(
                    "MyDb",
                    new[]
                    {
                        new TableSymbol("MyTable", "(a: real, b: real)"),
                    });
            _globals = GlobalState.Default.WithDatabase(db);
        }

        public void Foo()
        {
            var query = "MyTable | project a = a + b | where a > 10.0";
            var code = KustoCode.ParseAndAnalyze(query, _globals);

            // search syntax tree for references to specific columns
            var columnA = _globals.Database.Tables.First(t => t.Name == "MyTable").GetColumn("a");
            var referencesToA = code.Syntax.GetDescendants<NameReference>(n => n.ReferencedSymbol == columnA);

            foreach (var reference in referencesToA)
            {
                Console.WriteLine(reference.ToString());
            }
        }

        public void DumpTree(string query)
        {
            Console.WriteLine("Analyzing query:");
            Console.WriteLine(query);
            Console.WriteLine();

            var code = KustoCode.ParseAndAnalyze(query, _globals);

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
}