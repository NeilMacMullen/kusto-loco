using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using KustoLoco.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class GenericGenerator : IIncrementalGenerator
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
                var rawClassName = classDeclaration.Identifier.ValueText;
                var wrapperClassName = rawClassName + "Generic";
                var classStringLines = classDeclaration.ToString().Split('\n');
                var attributes =
                    CustomAttributeHelper<KustoGenericAttribute>.CreateFromNode(classDeclaration);
                if (!attributes.IsValid)
                    continue;
                var code = new CodeEmitter();
                var types = GetTypes(attributes);
                EmitHeader(code, classDeclaration);
                foreach (var type in types)
                {
                    var modified = classStringLines.Select(line => ProcessLine(attributes, rawClassName, type, line))
                        .ToArray();
                    var classOut = string.Join("\n", modified);


                    try
                    {
                        code.AppendLine(classOut);
                    }
                    catch (Exception ex)
                    {
                        code.LogException(ex);
                    }
                }

                context.AddSource($"{wrapperClassName}.g.cs", code.ToString());
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

    private string ProcessLine(CustomAttributeHelper<KustoGenericAttribute> attributes,
        string rawClassName,
        string type,
        string classString)
    {
        var containsGenericDirective =
            classString.Contains("//") && classString.Contains("GENERIC");


        var trimmedClassName = Regex.Replace(rawClassName, @"_.*$", "");

        var generic = "T";
        classString = classString.Replace(rawClassName, trimmedClassName);

        //get rid of attribute
        classString = Regex.Replace(classString, $@"\[{attributes.AttributeName()}.*\]", "");
        //var additionalAttrs = attributes.GetStringFor(nameof(KustoGenericAttribute.AdditionalAttributes));
        //if (additionalAttrs != "")
        //    code.AppendLine($"[{additionalAttrs}]");

        //get rid of constraints...
        classString = Regex.Replace(classString, $@"\s*where\s*{generic}\s*:.*", "");

        if (!containsGenericDirective && !classString.Contains("INPLACE"))
        {
            //replace simple <T> attributes
            classString = classString.Replace($"<{generic}>", $"Of{type}");
            //now look for more complicated examples
            var matches = Regex.Match(classString, @"\<(.+)\>");
            if (matches.Success)
            {
                var types = matches.Groups[1].Value.Split(',').Select(s => s.Trim()).ToArray();
                var trimmedTypes = types.Except(new[] { generic }).ToArray();
                if (trimmedTypes.Length < types.Length)
                {
                    var ttString = string.Join(",", trimmedTypes);
                    classString = classString.Replace(matches.Groups[0].Value, $"Of{type}<{ttString}>");
                }
            }
        }

        classString = classString.Replace("T.One", $"({type})1");
        classString = classString.Replace("T.Zero", $"({type})0");
        classString = classString.Replace($"{generic}?", $"{type}?");
        classString = classString.Replace($"({generic})", $"({type})");
        classString = classString.Replace($" {generic}[", $" {type}[");
        classString = classString.Replace($" {generic} ", $" {type} ");
        classString = classString.Replace($"{{{generic}}}", $"{type}");
        classString = classString.Replace($" {generic};", $" {type};");
        classString = classString.Replace($",{generic}>", $",{type}>");


        //take care of constructor
        classString = classString.Replace($"{trimmedClassName}(", $"{trimmedClassName}Of{type}(");

        //T?


        var condition = $"TYPE_{type.ToUpperInvariant()}";
        classString = classString.Replace(condition, "true");
        return classString;
    }

    private static void EmitHeader(CodeEmitter code, ClassDeclarationSyntax classDeclaration)
    {
        code.AppendLine("#nullable enable");
        var usingManager = new UsingsManager();
        usingManager.AddFromNode(classDeclaration);
        usingManager.Add("System");
        usingManager.Add("System.Text.Json.Nodes");
        usingManager.Add("CommunityToolkit.HighPerformance.Buffers");
        foreach (var u in usingManager.GetUsings())
            code.AppendLine(u);
        code.AppendStatement($"namespace {NodeHelpers.GetNamespaceFrom(classDeclaration)}");
    }

    private string[] GetTypes(CustomAttributeHelper<KustoGenericAttribute> attributes)
    {
        var all = Tokenise("bool,int,long,decimal,double,Guid,string,DateTime,TimeSpan,JsonNode");
        var numeric = Tokenise("int,long,decimal,double");
        var reference = Tokenise("string,JsonNode");
        var value = all.Except(reference).ToArray();
        var comparable = numeric.Concat(Tokenise("DateTime,TimeSpan")).ToArray();

        var userTypes = Tokenise(attributes.GetStringFor(nameof(KustoGenericAttribute.Types)));
        if (userTypes.Length == 0) return all;

        var wanted = new List<string>();
        foreach (var t in userTypes)
            switch (t)
            {
                case "all": wanted.AddRange(all); break;
                case "numeric": wanted.AddRange(numeric); break;
                case "reference": wanted.AddRange(reference); break;
                case "comparable": wanted.AddRange(comparable); break;
                case "value": wanted.AddRange(value); break;
                default:
                    wanted.Add(t);
                    break;
            }

        return wanted.Distinct().OrderBy(i => i).ToArray();
    }

    private string[] Tokenise(string a) => a.Split(',').Select(s => s.Trim()).ToArray();


    private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
        node is ClassDeclarationSyntax m && m.AttributeLists.Count > 0;

    private static ClassDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context) =>
        context.Node as ClassDeclarationSyntax;
}
