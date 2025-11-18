using System;

namespace KustoLoco.Core;

public readonly record struct ColumnResult(string Name, int Index, Type UnderlyingType)
{
    public static readonly ColumnResult Empty = new(string.Empty, -1, typeof(string));
}
