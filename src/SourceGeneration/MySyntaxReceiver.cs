using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGeneration
{
    internal class AttributedClassReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> found = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Note that the attribute name, is without the ending 'Attribute' e.g TestAttribute -> Test
            if (syntaxNode is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0)
            {
                var syntaxAttributes = cds.AttributeLists.SelectMany(e => e.Attributes)
                    .Where(e => e.Name.NormalizeWhitespace().ToFullString() == "KustoImplementation");


                if (syntaxAttributes.Any())
                {
                    found.Add(cds);
                }
            }
        }
    }
}