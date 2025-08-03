using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KustoLoco.SourceGeneration
{
    [Generator]
    public class KustoFunctionSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                //Debugger.Launch();
            }
#endif
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    (s, _) => IsSyntaxTargetForGeneration(s),
                    (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(m => m != null);

            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
                = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses,
                (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        public void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes,
            SourceProductionContext context)
        {
            try
            {
                foreach (var classDeclaration in classes.Distinct())
                {
                    var wrapperClassName = classDeclaration.Identifier.ValueText;

                    var attributes =
                        CustomAttributeHelper<KustoImplementationAttribute>.CreateFromNode(classDeclaration);
                    if (!attributes.IsValid)
                        continue;
                    var kustoAttributes=
                        new KustoImplementationAttributeDecoder(attributes);

                    var rawModifiers = classDeclaration.Modifiers.Select(m => m.ValueText).ToList();
                    if (!rawModifiers.Contains("sealed"))
                        rawModifiers.Insert(1,"sealed");
                    var modifiers = string.Join(" ",rawModifiers);
                    
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
                        code.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
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

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
            node is ClassDeclarationSyntax m && m.AttributeLists.Count > 0;

        private static ClassDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context) =>
            context.Node as ClassDeclarationSyntax;

        private void EmitHeader(CodeEmitter code, ClassDeclarationSyntax classDeclaration)
        {
            EmitUsings(code, classDeclaration);
            code.AppendStatement($"namespace {NodeHelpers.GetNamespaceFrom(classDeclaration)}");
            code.AppendLine("#nullable enable");
        }

        public void EmitUsings(CodeEmitter code, ClassDeclarationSyntax classDeclaration)
        {
            var usingManager = new UsingsManager();
            usingManager.AddFromNode(classDeclaration);
            usingManager.Add("Kusto.Language");
            usingManager.Add("Kusto.Language.Symbols");
            usingManager.Add("KustoLoco.Core.Util");
            usingManager.Add("KustoLoco.Core.Evaluation.BuiltIns");
            usingManager.Add("KustoLoco.Core.Evaluation");
            usingManager.Add("KustoLoco.Core.DataSource");
            usingManager.Add("KustoLoco.Core");
            usingManager.Add("KustoLoco.Core.DataSource.Columns");
            usingManager.Add("System.Diagnostics");
            usingManager.Add("System.Collections.Generic");
            usingManager.Add("KustoLoco.Core.DataSource.Columns");
            usingManager.Add("System.Collections.Concurrent");
            usingManager.Add("System.Threading.Tasks");
            usingManager.Add("System.Runtime.CompilerServices");
            usingManager.Add("KustoLoco.Core.Evaluation.BuiltIns.Impl");

            foreach (var u in usingManager.GetUsings()) code.AppendLine(u);
        }


        private string Opt(int i, int numRequired)
        {
            var m = i >= numRequired ? 0 : 1;
            return $"minOccurring:{m}";
        }

        private void EmitFunctionSymbol(CodeEmitter code,
            KustoImplementationAttributeDecoder attr,
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
            MethodDeclarationSyntax method, KustoImplementationAttributeDecoder attr)
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
                ParamGeneneration.BuildColumnarMethod(dbg, m,attr);
            if (m.KustoImplementationAttributeDecoder.ImplementationType == ImplementationType.Aggregate)
                ParamGeneneration.BuildInvokeMethod(dbg, m);
            return m;
        }


       

        
    }
}

public enum ImplementationType
{
    Function,
    Operator,
    Aggregate
}
