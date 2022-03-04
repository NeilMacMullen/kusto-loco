// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using BabyKusto.Core.Evaluation;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    public enum EvaluatedExpressionKind
    {
        Scalar,
        Columnar,
        Table,
    }

    internal abstract class IRExpressionNode : IRNode
    {
        private Dictionary<Type, object>? _cache;

        public IRExpressionNode(TypeSymbol resultType, EvaluatedExpressionKind resultKind)
        {
            ResultType = resultType ?? throw new ArgumentNullException(nameof(resultType));
            ResultKind = resultKind;
        }

        public TypeSymbol ResultType { get; }
        public EvaluatedExpressionKind ResultKind { get; }

        public T GetOrSetCache<T>(Func<T> factoryFunc)
        {
            if (_cache == null)
            {
                _cache = new Dictionary<Type, object>();
            }

            var typeKey = typeof(T);
            if (!_cache.TryGetValue(typeKey, out var item))
            {
                item = factoryFunc()!;
                _cache.Add(typeKey, item);
            }

            return (T)item;
        }

        protected static EvaluatedExpressionKind GetResultKind(TypeSymbol type)
        {
            if (type.IsScalar)
            {
                return EvaluatedExpressionKind.Scalar;
            }
            else if (type.IsTabular)
            {
                return EvaluatedExpressionKind.Table;
            }

            throw new InvalidOperationException($"Type cannot be mapped toa {nameof(EvaluatedExpressionKind)}: {type.Display}");
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
            for (int i = 0; i < arguments.ChildCount; i++)
            {
                var argument = arguments.GetChild(i);
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
}
