using System;

//TODO - Unfortunately this needs to be kept in step with
//SourceGenDependencies because I can't get the build
//system to understand that the external project is 
//referenced.
internal class KustoImplementationAttribute : Attribute
{
    public string Keyword { get; set; } = string.Empty;
}