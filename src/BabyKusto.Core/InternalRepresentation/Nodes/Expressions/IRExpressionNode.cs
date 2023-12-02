// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Immutable;
using Kusto.Language.Symbols;
using NLog;

namespace BabyKusto.Core.InternalRepresentation;

internal enum EvaluatedExpressionKind
{
    Scalar,
    Columnar,
    Table,
}

internal abstract class IRExpressionNode : IRNode
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private ImmutableDictionary<Type, object> _cache = ImmutableDictionary<Type, object>.Empty;

    public IRExpressionNode(TypeSymbol resultType, EvaluatedExpressionKind resultKind)
    {
        ResultType = resultType ?? throw new ArgumentNullException(nameof(resultType));
        ResultKind = resultKind;
    }

    public TypeSymbol ResultType { get; }
    public EvaluatedExpressionKind ResultKind { get; }

    public T GetOrSetCache<T>(Func<T> factoryFunc)
    {
        var typeKey = typeof(T);

        if (!_cache.TryGetValue(typeKey, out var item))
        {
            item = factoryFunc()!;
            _cache = _cache.SetItem(typeKey, item);
        }

        //It's not very clear if this can every occur in practice.  If it _can_ it would be good 
        //to understand the circumstances so treat it as a fatal error for now
        if (_cache.Count > 1)
            throw new InvalidOperationException("Unexpected implementation results - cache has two different entries");
        return (T)item;
    }

    protected static EvaluatedExpressionKind GetResultKind(TypeSymbol type)
    {
        if (type.IsScalar)
        {
            return EvaluatedExpressionKind.Scalar;
        }

        if (type.IsTabular)
        {
            return EvaluatedExpressionKind.Table;
        }

        throw new InvalidOperationException(
            $"Type cannot be mapped to {nameof(EvaluatedExpressionKind)}: {SchemaDisplay.GetText(type)}");
    }

    protected static EvaluatedExpressionKind GetResultKind(EvaluatedExpressionKind a, EvaluatedExpressionKind b)
    {
        if (a == b)
        {
            return a;
        }

        if ((a == EvaluatedExpressionKind.Scalar || b == EvaluatedExpressionKind.Scalar) &&
            (a == EvaluatedExpressionKind.Columnar || b == EvaluatedExpressionKind.Columnar))
        {
            return EvaluatedExpressionKind.Columnar;
        }

        throw new InvalidOperationException($"Incompatible expression kinds: {a}, {b}");
    }

    protected static EvaluatedExpressionKind GetResultKind(IRListNode<IRExpressionNode> arguments)
    {
        if (arguments.ChildCount == 0)
        {
            return EvaluatedExpressionKind.Scalar;
        }

        EvaluatedExpressionKind? resultKind = null;
        for (var i = 0; i < arguments.ChildCount; i++)
        {
            var argument = arguments.GetTypedChild(i);
            if (argument.ResultKind == EvaluatedExpressionKind.Table)
            {
                // Tabular inputs are special and don't participate in row-scope result kind decisions
                continue;
            }

            resultKind =
                resultKind.HasValue
                    ? GetResultKind(resultKind.Value, argument.ResultKind)
                    : argument.ResultKind;
        }

        if (!resultKind.HasValue)
        {
            // TODO: this is absurd
            return EvaluatedExpressionKind.Table;
            //throw new InvalidOperationException("Could not determine result kind from argument types.");
        }

        return resultKind.Value;
    }
}