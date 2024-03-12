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
            try
            {
                var syntaxReceiver = (AttributedClassReceiver)context.SyntaxReceiver;
                Console.WriteLine("Code generator running...");
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
                    var endMethods = classDeclaration.Members.OfType<MethodDeclarationSyntax>()
                        .Where(m => m.Identifier.ValueText.EndsWith("ImplFinish"))
                        .ToArray();

                    var implMethodClasses = new List<ImplementationMethod>();

                    foreach (var implMethod in implementationMethods)
                    {
                        var code = new CodeEmitter();

                        var className = wrapperClassName + implMethod.Identifier.ValueText;

                        EmitHeader(code, classDeclaration);
                        code.AppendLine($"{modifiers} class {className} : {kustoAttributes.BaseClassName}");
                        code.EnterCodeBlock();
                        var m = GenerateImplementation(code, className, implMethod, kustoAttributes);
                        implMethodClasses.Add(m);
                        code.AppendLine(implMethod.ToFullString());
                        foreach (var endMethod in endMethods.Where(e => e.Identifier.ValueText.Contains(m.Name)))
                            code.AppendLine(endMethod.ToFullString());
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
                        var overloadWrapperName = kustoAttributes.OverloadWrapperName();
                        wrapperCode.AppendLine($"public static {overloadWrapperName} S=new {overloadWrapperName}(");

                        var overloads = string.Join(",",
                            implMethodClasses.Select(s => $"{wrapperClassName}{s.Name}.Overload"));
                        wrapperCode.AppendLine(overloads);


                        wrapperCode.AppendStatement(")");
                        wrapperCode.AppendLine(
                            $"public static void Register(Dictionary<{kustoAttributes.SymbolTypeName},{overloadWrapperName}> f)");

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
            catch (Exception e)
            {
                var c = new CodeEmitter();
                c.AppendStatement("SOURCE GENERATOR EXCEPTION");
                c.LogException(e);
                context.AddSource("sourcegeneratorexception", c.ToString());
                // throw;
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
            code.AppendStatement("using KustoLoco.Core.Util");
            code.AppendStatement("using System.Collections.Generic");
            code.AppendStatement("using KustoLoco.Core.Evaluation.BuiltIns");
            code.AppendStatement("using KustoLoco.Core.Evaluation");
            code.AppendStatement("using KustoLoco.Core.DataSource");
            code.AppendStatement("using KustoLoco.Core");


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
            if (m.HasScalar)
                ParamGeneneration.BuildScalarMethod(dbg, m);
            if (m.HasColumnar)
                ParamGeneneration.BuildColumnarMethod(dbg, m);
            if (m.AttributeDecoder.ImplementationType == ImplementationType.Aggregate)
                ParamGeneneration.BuildInvokeMethod(dbg, m);
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

public enum ImplementationType
{
    Function,
    Operator,
    Aggregate
}