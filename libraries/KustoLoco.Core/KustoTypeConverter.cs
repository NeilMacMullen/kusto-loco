using System;

namespace KustoLoco.Core;

/// <summary>
///     Allows type conversion when importing data from DTOs
/// </summary>
/// <remarks>
///     To convert a custom class (eg MyClass) into a recognised CLR type (eg string),
///     Use new KustoTypeConverter &lt;MyClass,string&gt;((_,my)=>$"{my.Name}--{my.Size}")
/// </remarks>
public class KustoTypeConverter<TSource, TTarget> : IKustoTypeConverter
{
    private readonly Func<string, TSource, TTarget> _converterWithName = (_, _) => default!;
    private readonly Func<TSource, TTarget> _converterWithoutName = _ => default!;
    private readonly bool _usingName;

    /// <summary>
    ///     Allows type conversion when importing data from DTOs
    /// </summary>
    /// <remarks>
    ///     To convert a custom class (eg MyClass) into a recognised CLR type (eg string),
    ///     Use new KustoTypeConverter &lt;MyClass,string&gt;((_,my)=>$"{my.Name}--{my.Size}")
    /// </remarks>
    public KustoTypeConverter(Func<string, TSource, TTarget> converter)
    {
        _converterWithName = converter;
        _usingName = true;
    }

    public KustoTypeConverter(Func<TSource, TTarget> converter)
    {
        _converterWithoutName = converter;
        _usingName = false;
    }

    public Type SourceType { get; } = typeof(TSource);
    public Type TargetType { get; set; } = typeof(TTarget);

    public object? Convert(string name, object? obj)
    {
        if (obj is not TSource source) return null;
        return _usingName
            ? _converterWithName(name, source)
            : _converterWithoutName(source);
    }
}
