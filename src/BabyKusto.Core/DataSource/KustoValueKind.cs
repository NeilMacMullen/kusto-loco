using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core
{
    public enum KustoValueKind
    {
        Bool,

        Int,

        Long,

        Real,

        Decimal,

        String,

        DateTime,

        TimeSpan,

        Guid,

        Type,

        Dynamic
    }

    public static class TypeSymbolExtensions
    {
        public static KustoValueKind ToKustoValueKind(this TypeSymbol typeSymbol)
        {
            if (ReferenceEquals(typeSymbol, ScalarTypes.Bool))
            {
                return KustoValueKind.Bool;
            }
            else if (ReferenceEquals(typeSymbol, ScalarTypes.Int))
            {
                return KustoValueKind.Int;
            }
            else if (ReferenceEquals(typeSymbol, ScalarTypes.Long))
            {
                return KustoValueKind.Long;
            }
            else if (ReferenceEquals(typeSymbol, ScalarTypes.Real))
            {
                return KustoValueKind.Real;
            }
            else if (ReferenceEquals(typeSymbol, ScalarTypes.Decimal))
            {
                return KustoValueKind.Decimal;
            }
            else if (ReferenceEquals(typeSymbol, ScalarTypes.String))
            {
                return KustoValueKind.String;
            }
            else if (ReferenceEquals(typeSymbol, ScalarTypes.DateTime))
            {
                return KustoValueKind.DateTime;
            }
            else if (ReferenceEquals(typeSymbol, ScalarTypes.TimeSpan))
            {
                return KustoValueKind.TimeSpan;
            }
            else if (ReferenceEquals(typeSymbol, ScalarTypes.Guid))
            {
                return KustoValueKind.Guid;
            }
            else if (ReferenceEquals(typeSymbol, ScalarTypes.Type))
            {
                return KustoValueKind.Type;
            }
            else if (ReferenceEquals(typeSymbol, ScalarTypes.Type))
            {
                return KustoValueKind.Dynamic;
            }

            throw new InvalidOperationException("Unsupported scalar type");
        }
    }
}
