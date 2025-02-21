// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentAssertions;
using Kusto.Language.Symbols;
using Xunit;

namespace KustoLoco.Core.Extensions.Tests;

public class TypeSymbolExtensionsTests
{
    public static object[][] NonDynamicTypes =
    [
        [ScalarTypes.Bool],
        [ScalarTypes.Int],
        [ScalarTypes.Long],
        [ScalarTypes.Real],
        [ScalarTypes.String],
        [ScalarTypes.TimeSpan],
        [ScalarTypes.DateTime]
    ];

    public static object[][] DynamicTypes =
    [
        [ScalarTypes.Dynamic],
        [ScalarTypes.DynamicBag],
        [ScalarTypes.DynamicArrayOfLong],
        [ScalarTypes.DynamicArrayOfReal],
        [ScalarTypes.DynamicArrayOfArray],
        [ScalarTypes.DynamicArrayOfString],
        [ScalarTypes.GetDynamic(ScalarTypes.DateTime)],
        [ScalarTypes.GetDynamicArray(ScalarTypes.DateTime)]
    ];

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
