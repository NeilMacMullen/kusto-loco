using System;

namespace KustoLoco.SourceGeneration.Attributes
{
    public class KustoImplementationAttribute : Attribute
    {
        public string Keyword { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class KustoGenericAttribute : Attribute
    {
        public string Types { get; set; } = string.Empty;
        public string AdditionalAttributes { get; set; } = string.Empty;
    }
}
