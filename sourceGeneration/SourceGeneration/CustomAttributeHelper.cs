using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KustoLoco.SourceGeneration
{
    /// <summary>
    /// A class which helps us extract the arguments from custom properties
    /// </summary>
    /// <remarks>
    /// IsValid will be false if the attribute wasn't recognised
    /// </remarks>
    public class CustomAttributeHelper<T>
        where T : Attribute
    {
        public readonly bool IsValid;
        private readonly Dictionary<string, string> _attributes;

        private CustomAttributeHelper(bool isValid,Dictionary<string, string> attributes)
        {
            IsValid = isValid;
            _attributes = attributes;
        }

        /// <summary>
        /// The name of the attribute (without Attribute suffix)
        /// </summary>
        /// <returns></returns>
        public static string Name()
        {
            var fullName = typeof(T).Name;
            return fullName.EndsWith("Attribute")
                ? fullName.Substring(0, fullName.Length - "Attribute".Length)
                : fullName;
        }
        public string AttributeName()
        => Name();
        
        public string Unquote(string other)
        {
            if (other.StartsWith("\""))
                other = other.Substring(1);
            if (other.EndsWith("\""))
                other = other.Substring(0, other.Length - 1);
            return other;
        }
        /// <summary>
        /// Get the value of an attribute property as a string
        /// </summary>
        public string GetStringFor(string propertyName) =>
            _attributes.TryGetValue(propertyName, out var p) ? Unquote(p) : string.Empty;

        
        public string Dump()
        {
            var sb = new StringBuilder();
            foreach (var a in _attributes) sb.AppendLine($"'{a.Key}' => '{a.Value}'");
            return sb.ToString();
        }

        /// <summary>
        /// Creates a helper from the class syntax
        /// </summary>
        public static CustomAttributeHelper<T> CreateFromNode(ClassDeclarationSyntax classDeclaration)
        {
            var attributes = classDeclaration.AttributeLists
                .SelectMany(e => e.Attributes)
                .Where(e => e.Name.NormalizeWhitespace().ToFullString() ==
                            CustomAttributeHelper<T>.Name())
                .ToArray();

            if (!attributes.Any())
                return new CustomAttributeHelper<T>(false,new Dictionary<string, string>
                {
                    ["error"] = "no attributes"
                });
            var dict = new Dictionary<string, string>();

            var sa = attributes.First();
            if (sa.ArgumentList != null)
                dict = sa.ArgumentList.Arguments.ToDictionary(
                    a => a.NameEquals.Name.Identifier.ValueText,
                    a => a.Expression.ToString()
                );
           
            return new CustomAttributeHelper<T>(true,dict);
        }

    }
}
