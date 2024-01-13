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
            var dbg = new CodeAcccumulator();
            foreach (var classDeclaration in syntaxReceiver.found)
            {
                var className = classDeclaration.Identifier.ValueText;
                var implMethods = classDeclaration.Members.OfType<MethodDeclarationSyntax>()
                    .Where(m => m.Identifier.ValueText.EndsWith("Impl"))
                    .ToArray();


                dbg.AppendStatement("using System.Diagnostics");
                dbg.AppendStatement("using BabyKusto.Core.Util");
                dbg.AppendStatement("namespace BabyKusto.Core.Evaluation.BuiltIns.Impl");
                dbg.AppendLine("#nullable enable");

                dbg.AppendLine($"public partial class {className}");
                dbg.EnterCodeBlock();
                foreach (var implMethod in implMethods)
                {
                    GenerateImplementation(dbg, implMethod);
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

        private static void GenerateImplementation(CodeAcccumulator dbg, MethodDeclarationSyntax method)
        {
            var parameters = method.ParameterList.Parameters
                .Select((p, i) => new Param(i, p.Identifier.ValueText, p.Type.ToFullString()))
                .ToArray();

            var ret = new Param(0, string.Empty, method.ReturnType.ToFullString());

            var m = new ImplementationMethod(method.Identifier.ValueText, ret, parameters);
            ParamGeneneration.BuildScalarMethod(dbg, m);
            ParamGeneneration.BuildColumnarMethod(dbg, m);
        }
    }

    public class ImplementationMethod
    {
        public readonly Param[] _arguments;
        public readonly string _name;
        public readonly Param _returnType;

        public ImplementationMethod(string name, Param returnType, Param[] arguments)
        {
            _name = name;
            _returnType = returnType;
            _arguments = arguments;
        }
    }
}