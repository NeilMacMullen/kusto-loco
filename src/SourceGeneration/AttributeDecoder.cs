using System;
using SourceGeneration;

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

    public bool IsFuncOrOp => ImplementationType == ImplementationType.Function ||
                              ImplementationType == ImplementationType.Operator;


    public string OverloadName()
    {
        switch (ImplementationType)

        {
            case ImplementationType.Operator:
            case ImplementationType.Function:
                return "ScalarOverloadInfo";
            default:
                return "not yet implemented";
        }
    }
}