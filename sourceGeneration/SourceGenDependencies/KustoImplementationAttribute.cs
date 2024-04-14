using System;

namespace KustoLoco.SourceGeneration.Attributes
{
    public class KustoImplementationAttribute : Attribute
    {
        public string Keyword { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
