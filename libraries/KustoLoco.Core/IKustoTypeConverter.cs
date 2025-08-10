using System;

namespace KustoLoco.Core;

public interface IKustoTypeConverter
{
    public Type SourceType { get; }
    public Type TargetType { get; }
    public object? Convert(string name,object? obj);
}
