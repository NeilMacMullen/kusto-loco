// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentAssertions;
using Kusto.Language.Symbols;
using Xunit;

namespace KustoLoco.Core.Extensions.Tests;

public class TypeSymbolExtensionsTests
{
    public static object[][] NonDynamicTypes =
    {
        new object[] { ScalarTypes.Bool },
        new object[] { ScalarTypes.Int },
        new object[] { ScalarTypes.Long },
        new object[] { ScalarTypes.Real },
        new object[] { ScalarTypes.String },
        new object[] { ScalarTypes.TimeSpan },
        new object[] { ScalarTypes.DateTime },
    };

    public static object[][] DynamicTypes =
    {
        new object[] { ScalarTypes.Dynamic },
        new object[] { ScalarTypes.DynamicBag },
        new object[] { ScalarTypes.DynamicArrayOfLong },
        new object[] { ScalarTypes.DynamicArrayOfReal },
        new object[] { ScalarTypes.DynamicArrayOfArray },
        new object[] { ScalarTypes.DynamicArrayOfString },
        new object[] { ScalarTypes.GetDynamic(ScalarTypes.DateTime) },
        new object[] { ScalarTypes.GetDynamicArray(ScalarTypes.DateTime) },
    };

    [Theory]
    [MemberData(nameof(NonDynamicTypes))]
    public void NonDynamicTypes_Works(TypeSymbol type)
    {
        // Act
        var simplified = type.Simplify();

        // Assert
        simplified.Should().BeSameAs(type);
    }

    [Theory]
    [MemberData(nameof(DynamicTypes))]
    public void DynamicTypes_Works(TypeSymbol type)
    {
        // Act
        var simplified = type.Simplify();

        // Assert
        simplified.Should().BeSameAs(ScalarTypes.Dynamic);
    }
}