// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal enum SortDirections
    {
        Asc,
        Desc,
        GrannyAsc,
        GrannyDesc,
    }

    internal enum NullsDirections
    {
        First,
        Last,
    }

    internal class IROrderedExpressionNode : IRExpressionNode
    {
        public IROrderedExpressionNode(IRExpressionNode expression, SortDirections sortDirection, NullsDirections nullsDirection, TypeSymbol resultType)
            : base(resultType, expression.ResultKind)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            SortDirection = sortDirection;
            NullsDirection = nullsDirection;
        }

        public IRExpressionNode Expression { get; }
        public SortDirections SortDirection { get; }
        public NullsDirections NullsDirection { get; }

        public override int ChildCount => 1;
        public override IRNode GetChild(int index) =>
            index switch
            {
                0 => Expression,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitOrderedExpression(this, context);
        }

        public override string ToString()
        {
            return $"OrderedExpression {FormatDirection(SortDirection)} nulls {FormatNullsDirection(NullsDirection)}: {ResultType.Display}";

            static string FormatDirection(SortDirections sortDirection) =>
                sortDirection switch
                {
                    SortDirections.Asc => "asc",
                    SortDirections.Desc => "desc",
                    SortDirections.GrannyAsc => "granny-asc",
                    SortDirections.GrannyDesc => "granny-desc",
                    _ => sortDirection.ToString(),
                };
            static string FormatNullsDirection(NullsDirections nullsDirection) =>
                nullsDirection switch
                {
                    NullsDirections.First => "first",
                    NullsDirections.Last => "last",
                    _ => nullsDirection.ToString(),
                };
        }
    }
}
