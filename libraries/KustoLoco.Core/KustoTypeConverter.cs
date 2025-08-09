using System;

namespace KustoLoco.Core;


/// <summary>
/// Allows type conversion when importing data from DTOs
/// </summary>
/// <remarks>
/// To convert a custom class (eg MyClass) into a recognised CLR type (eg string),
/// Use new KustoTypeConverter &lt;MyClass,string&gt;(my=>$"{my.Name}--{my.Size}") 
/// </remarks>
public class KustoTypeConverter<TSource, TTarget>(Func<TSource, TTarget> converter) : IKustoTypeConverter
{
    public Type SourceType { get; } = typeof(TSource);
    public Type TargetType { get; set; } = typeof(TTarget);
    public object? Convert(object? obj)
    {
        if (obj is not TSource source) return null;
        return converter(source);
    }
}
