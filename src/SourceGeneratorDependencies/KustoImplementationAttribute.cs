using System;

namespace SourceGeneratorDependencies
{
    public class KustoImplementationAttribute : Attribute
    {
        public string Keyword { get; set; } = string.Empty;
    }
}