using System;


namespace KustoLoco.SourceGeneration
{
    /// <summary>
    /// A helper specifically for the KustoImplementationAttribute
    /// </summary>
    public class KustoImplementationAttributeDecoder
    {
        public ImplementationType ImplementationType;
        public bool IsBuiltIn;

        public string SymbolName;

        internal KustoImplementationAttributeDecoder(CustomAttributeHelper<KustoImplementationAttribute> attr)
        {
            Partition= !attr.GetStringFor(nameof(KustoImplementationAttribute.Partition)).Equals("false");
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
        public bool Partition { get; }
        public string SymbolTypeName
        {
            get
            {
                switch (ImplementationType)
                {
                    case ImplementationType.Function:
                    case ImplementationType.Aggregate:
                        return "FunctionSymbol";
                    case ImplementationType.Operator:
                        return "OperatorSymbol";
                    default: return "no symbol";
                }
            }
        }

        public bool IsFuncOrOp => ImplementationType == ImplementationType.Function ||
                                  ImplementationType == ImplementationType.Operator;

        
        public string BaseClassName
        {
            get
            {
                switch (ImplementationType)
                {
                    case ImplementationType.Function:

                    case ImplementationType.Operator:
                        return "IScalarFunctionImpl";
                    case ImplementationType.Aggregate:
                        return "IAggregateImpl";
                    default: return "no symbol";
                }
            }
        }


        public string OverloadName()
        {
            switch (ImplementationType)

            {
                case ImplementationType.Operator:
                case ImplementationType.Function:
                    return "ScalarOverloadInfo";
                case ImplementationType.Aggregate:
                    return "AggregateOverloadInfo";
                default:
                    return "not yet implemented";
            }
        }

        public string OverloadWrapperName()
        {
            switch (ImplementationType)

            {
                case ImplementationType.Operator:
                case ImplementationType.Function:
                    return "ScalarFunctionInfo";
                case ImplementationType.Aggregate:
                    return "AggregateInfo";
                default:
                    return "not yet implemented";
            }
        }

    }
}
