using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGeneration
{
    [Generator]
    public class KustoFunctionSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxReceiver = (AttributedClassReceiver)context.SyntaxReceiver;

            foreach (var classDeclaration in syntaxReceiver.found)
            {
                var dbg = new CodeAcccumulator();
                var className = classDeclaration.Identifier.ValueText;

                var modifiers = string.Join(" ", classDeclaration.Modifiers.Select(m => m.ValueText));
                var implMethods = classDeclaration.Members.OfType<MethodDeclarationSyntax>()
                    .Where(m => m.Identifier.ValueText.EndsWith("Impl"))
                    .ToArray();

                dbg.AppendStatement("using Kusto.Language.Symbols");
                dbg.AppendStatement("using System.Diagnostics");
                foreach (var u in GetUsingList(classDeclaration))
                {
                    dbg.AppendLine(u);
                }

                dbg.AppendStatement($"namespace {GetNamespaceFrom(classDeclaration)}");

                dbg.AppendLine("#nullable enable");

                dbg.AppendLine($"{modifiers} class {className}");
                dbg.EnterCodeBlock();
                foreach (var implMethod in implMethods)
                {
                    GenerateImplementation(dbg, className, implMethod);
                }

                dbg.ExitCodeBlock();
                // Add the source code to the compilation
                context.AddSource($"{className}.g.cs", dbg.ToString());
            }
        }


        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                //Debugger.Launch();
            }
#endif
            context.RegisterForSyntaxNotifications(() => new AttributedClassReceiver());
        }

        private static void GenerateImplementation(CodeAcccumulator dbg, string className,
            MethodDeclarationSyntax method)
        {
            var parameters = method.ParameterList.Parameters
                .Select((p, i) => new Param(i, p.Identifier.ValueText, p.Type.ToFullString()))
                .ToArray();

            var ret = new Param(0, string.Empty, method.ReturnType.ToFullString());

            var m = new ImplementationMethod(className, method.Identifier.ValueText, ret, parameters);
            ParamGeneneration.BuildOverloadInfo(dbg, m);
            ParamGeneneration.BuildScalarMethod(dbg, m);
            ParamGeneneration.BuildColumnarMethod(dbg, m);
        }


        public static string GetNamespaceFrom(SyntaxNode s)
        {
            while (true)
            {
                if (s == null) return "No namespace found!";
                if (s is BaseNamespaceDeclarationSyntax ns) return ns.Name.ToString();
                s = s.Parent;
            }
        }

        public static string[] GetUsingList(SyntaxNode s)
        {
            var usings = new List<string>();
            while (true)
            {
                if (s == null) return usings.ToArray();
                if (s is CompilationUnitSyntax cu)
                {
                    usings.AddRange(cu.Usings.Select(c => c.ToString()));
                }

                s = s.Parent;
            }
        }
    }

    public class ImplementationMethod
    {
        public readonly Param[] Arguments;
        public readonly string Name;
        public readonly Param ReturnType;

        public ImplementationMethod(string className, string name, Param returnType, Param[] arguments)
        {
            ClassName = className;
            Name = name;
            ReturnType = returnType;
            Arguments = arguments;
        }

        public string ClassName { get; }
    }
}