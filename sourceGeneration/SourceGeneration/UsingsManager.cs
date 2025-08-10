using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace KustoLoco.SourceGeneration
{
    public class UsingsManager
    {
        private List<string> _usingList = new List<string>();

        public UsingsManager()
        {
        }
        public void AddFromNode(SyntaxNode node)
        {
            
            var s = node;
            while (true)
            {
                switch (s)
                {
                    case null:
                        return;
                    case CompilationUnitSyntax cu:
                        _usingList.AddRange(cu.Usings.Select(c => c.ToString()));
                        break;
                }

                s = s.Parent;
            }
        }

        public void Add(string path)
        {
          _usingList.Add(path);
        }

        public IEnumerable<string> GetUsings()
        {
            return _usingList.Select(u => u.Replace("using", "").Replace(";","").Trim())
                .Distinct()
                .OrderBy(u => u)
                .Select(u => $"using {u};");
        }
    }
}
