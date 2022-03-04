// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRDataTableExpression : IRExpressionNode
    {
        public IRDataTableExpression(object?[] data, TypeSymbol resultType)
            : base(resultType, EvaluatedExpressionKind.Table)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            if (data.Length % resultType.Members.Count != 0)
            {
                throw new ArgumentException($"Invalid number of columns in data. Expected a multiple of {resultType.Members.Count}, found {data.Length}.");
            }
        }

        public object?[] Data { get; }

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitDataTableExpression(this, context);
        }

        public override string ToString()
        {
            return $"DataTableExpression: {ResultType.Display}";
        }
    }
}
