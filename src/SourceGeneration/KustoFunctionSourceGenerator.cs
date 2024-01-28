using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGeneration;

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

                var kustoAttributes =
                    new AttributeDecoder(
                        AttributeAsHelper<KustoImplementationAttribute>(classDeclaration));


                var modifiers = string.Join(" ", classDeclaration.Modifiers.Select(m => m.ValueText));

                var implementationMethods = classDeclaration.Members.OfType<MethodDeclarationSyntax>()
                    .Where(m => m.Identifier.ValueText.EndsWith("Impl"))
                    .ToArray();

                var implMethodClasses = new List<ImplementationMethod>();

                foreach (var implMethod in implementationMethods)
                {
                    var code = new CodeEmitter();

                    var className = wrapperClassName + implMethod.Identifier.ValueText;

                    EmitHeader(code, classDeclaration);
                    code.AppendLine($"{modifiers} class {className} : IScalarFunctionImpl");
                    code.EnterCodeBlock();
                    var m = GenerateImplementation(code, className, implMethod, kustoAttributes);
                    implMethodClasses.Add(m);
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
                    EmitFunctionSymbol(wrapperCode, kustoAttributes, implMethodClasses);
                    //create the registration
                    wrapperCode.AppendLine(@"public static ScalarFunctionInfo S=new ScalarFunctionInfo(");

                    var overloads = string.Join(",",
                        implMethodClasses.Select(s => $"{wrapperClassName}{s.Name}.Overload"));
                    wrapperCode.AppendLine(overloads);


                    wrapperCode.AppendStatement(")");
                    wrapperCode.AppendLine(
                        $"public static void Register(Dictionary<{kustoAttributes.SymbolTypeName},ScalarFunctionInfo> f)");

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

        private string Opt(int i, int numRequired)
        {
            var m = i >= numRequired ? 0 : 1;
            return $"minOccurring:{m}";
        }

        private void EmitFunctionSymbol(CodeEmitter code,
            AttributeDecoder attr,
            List<ImplementationMethod> implementationMethods)
        {
            if (attr.IsBuiltIn)
            {
                code.AppendStatement($"public static readonly {attr.SymbolTypeName} Func = {attr.SymbolName}");
            }
            else
            {
                //assume it's a function
                //figure out return type...
                var returnType = ParamGeneneration.ScalarType(implementationMethods.First().ReturnType);
                var longest = implementationMethods.OrderBy(f => f.TypedArguments.Length)
                    .Last();
                var shortest = implementationMethods.Min(f => f.TypedArguments.Length);

                var args = longest.TypedArguments
                    .Select((a, i) =>
                        $"new Parameter(\"{a.Name}\", ScalarTypes.{ParamGeneneration.ScalarType(a)},{Opt(i, shortest)})")
                    .ToArray();

                code.AppendLine($"public static readonly {attr.SymbolTypeName} Func =");
                code.AppendLine($"new {attr.SymbolTypeName}(\"{attr.SymbolName}\", ");
                code.AppendLine($"ScalarTypes.{returnType},");

                code.AppendLine(string.Join(",", args));
                code.AppendLine(" ).ConstantFoldable()");
                code.AppendStatement(" .WithResultNameKind(ResultNameKind.None)");
            }
        }

        private static ImplementationMethod GenerateImplementation(CodeEmitter dbg, string className,
            MethodDeclarationSyntax method, AttributeDecoder attr)
        {
            var parameters = method.ParameterList.Parameters
                .Select((p, i) => new Param(i, p.Identifier.ValueText, p.Type.ToFullString()))
                .ToArray();

            var ret = new Param(0, string.Empty, method.ReturnType.ToFullString());

            var m = new ImplementationMethod(className, method.Identifier.ValueText, ret, parameters, attr);
            ParamGeneneration.BuildOverloadInfo(dbg, m);
            ParamGeneneration.BuildScalarMethod(dbg, m);
            ParamGeneneration.BuildColumnarMethod(dbg, m);
            return m;
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
}

public class AttributeDecoder
{
    public ImplementationType ImplementationType;
    public bool IsBuiltIn;

    public string SymbolName;

    internal AttributeDecoder(CustomAttributeHelper<KustoImplementationAttribute> attr)
    {
        var funcSymbol = attr.GetStringFor(nameof(KustoImplementationAttribute.Keyword));
        SymbolName = funcSymbol;

        if (funcSymbol.Contains("Functions"))
        {
            IsBuiltIn = true;
            ImplementationType = ImplementationType.Function;
        }
        else if (funcSymbol.Contains("Operators"))
        {
            IsBuiltIn = true;
            ImplementationType = ImplementationType.Operator;
        }
        else if (funcSymbol.Contains("Aggregates"))
        {
            IsBuiltIn = true;
            ImplementationType = ImplementationType.Aggregate;
        }
        else
        {
            var category = attr.GetStringFor(nameof(KustoImplementationAttribute.Category));
            if (category == string.Empty)
                ImplementationType = ImplementationType.Function;
            else
                ImplementationType = (ImplementationType)Enum.Parse(typeof(ImplementationType), category);
        }
    }

    public string SymbolTypeName => $"{ImplementationType}Symbol";

    public string OverloadName()
    {
        switch (ImplementationType)

        {
            case ImplementationType.Operator:
            case ImplementationType.Function: return "ScalarOverloadInfo";
            default:
                return "not yet implemented";
        }
    }
}

public enum ImplementationType
{
    Function,
    Operator,
    Aggregate
}