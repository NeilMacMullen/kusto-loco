using System;
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
                var wrapperClassName = classDeclaration.Identifier.ValueText;

                var kustoAttributes = AttributeAsHelper<KustoImplementationAttribute>(classDeclaration);


                var modifiers = string.Join(" ", classDeclaration.Modifiers.Select(m => m.ValueText));

                var implementationMethods = classDeclaration.Members.OfType<MethodDeclarationSyntax>()
                    .Where(m => m.Identifier.ValueText.EndsWith("Impl"))
                    .ToArray();

                var implMethodClasses = new List<string>();
                foreach (var implMethod in implementationMethods)
                {
                    var code = new CodeEmitter();

                    var className = wrapperClassName + implMethod.Identifier.ValueText;
                    implMethodClasses.Add(className);
                    EmitHeader(code, classDeclaration);
                    code.AppendLine($"{modifiers} class {className} : IScalarFunctionImpl");
                    code.EnterCodeBlock();
                    GenerateImplementation(code, className, implMethod);

                    code.AppendLine(implMethod.ToFullString());
                    code.ExitCodeBlock();
                    // Add the source code to the compilation
                    context.AddSource($"{className}.g.cs", code.ToString());
                }

                var wrapperCode = new CodeEmitter();
                try
                {
                    EmitHeader(wrapperCode, classDeclaration);
                    wrapperCode.AppendLine($"{modifiers} class {wrapperClassName}");
                    wrapperCode.EnterCodeBlock();
                    EmitFunctionSymbol(wrapperCode, kustoAttributes);
                    //create the registration
                    wrapperCode.AppendLine(@"public static ScalarFunctionInfo S=new ScalarFunctionInfo(");

                    var overloads = string.Join(",", implMethodClasses.Select(s => $"{s}.Overload"));
                    wrapperCode.AppendLine(overloads);


                    wrapperCode.AppendStatement(")");
                    wrapperCode.AppendLine(
                        "public static void Register(Dictionary<FunctionSymbol,ScalarFunctionInfo> f)");

                    wrapperCode.AppendStatement("=> f.Add(Func,S)");

                    wrapperCode.ExitCodeBlock();
                }
                catch (Exception ex)
                {
                    wrapperCode.LogException(ex);
                }

                // Add the source code to the compilation
                if (modifiers.Contains("partial"))
                    context.AddSource($"{wrapperClassName}.g.cs", wrapperCode.ToString());
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

        private void EmitHeader(CodeEmitter code, ClassDeclarationSyntax classDeclaration)
        {
            EmitUsings(code, classDeclaration);
            code.AppendStatement($"namespace {GetNamespaceFrom(classDeclaration)}");
            code.AppendLine("#nullable enable");
        }

        public void EmitUsings(CodeEmitter code, ClassDeclarationSyntax classDeclaration)
        {
            code.AppendStatement("using Kusto.Language");
            code.AppendStatement("using Kusto.Language.Symbols");
            code.AppendStatement("using System.Diagnostics");
            code.AppendStatement("using BabyKusto.Core.Util");
            code.AppendStatement("using System.Collections.Generic");
            code.AppendStatement("using BabyKusto.Core.Evaluation.BuiltIns");
            code.AppendStatement("using BabyKusto.Core.Evaluation");
            code.AppendStatement("using BabyKusto.Core");


            foreach (var u in GetUsingList(classDeclaration))
            {
                code.AppendLine(u);
            }
        }

        private static CustomAttributeHelper<T> AttributeAsHelper<T>(ClassDeclarationSyntax classDeclaration)
            where T : Attribute
        {
            var attributes = classDeclaration.AttributeLists
                .SelectMany(e => e.Attributes)
                .Where(e => e.Name.NormalizeWhitespace().ToFullString() ==
                            CustomAttributeHelper<T>.Name())
                .ToArray();

            if (!attributes.Any())
            {
                return new CustomAttributeHelper<T>(new Dictionary<string, string>
                {
                    ["error"] = "no attributes"
                });
            }

            var dict = new Dictionary<string, string>
            {
                ["error"] = "null ArgumentList"
            };

            var sa = attributes.First();
            if (sa.ArgumentList != null)
                dict = sa.ArgumentList.Arguments.ToDictionary(
                    a => a.NameEquals.Name.Identifier.ValueText,
                    a => a.Expression.ToString()
                );

            return new CustomAttributeHelper<T>(dict);
        }

        private void EmitFunctionSymbol(CodeEmitter code, CustomAttributeHelper<KustoImplementationAttribute> attr)
        {
            var funcSymbol = attr.GetStringFor(nameof(KustoImplementationAttribute.Keyword));
            if (funcSymbol.Contains("Functions"))
            {
                code.AppendStatement($"public static readonly FunctionSymbol Func = {funcSymbol}");
            }
            else
            {
                /*
                  new FunctionSymbol("debug_emit", ScalarTypes.Int,
                           new Parameter("value1", ScalarTypes.String)
                       ).ConstantFoldable()
                       .WithResultNameKind(ResultNameKind.None);
                 */
                code.AppendLine("public static readonly FunctionSymbol Func =");
                code.AppendLine($"new FunctionSymbol(\"{funcSymbol}\", ");
                code.AppendLine("ScalarTypes.String,");
                code.AppendLine("new Parameter(\"value1\", ScalarTypes.Int)");
                code.AppendLine(" ).ConstantFoldable()");
                code.AppendStatement(" .WithResultNameKind(ResultNameKind.None)");
            }
        }

        private static void GenerateImplementation(CodeEmitter dbg, string className,
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