namespace KustoSupport;

public readonly record struct ColumnResult(string Name, int Index, Type UnderlyingType);