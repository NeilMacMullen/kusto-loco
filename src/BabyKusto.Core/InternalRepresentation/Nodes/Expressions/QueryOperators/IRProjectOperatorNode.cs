// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRProjectOperatorNode : IRQueryOperatorNode
    {

        public IRProjectOperatorNode(IRListNode<IROutputColumnNode> columns, TypeSymbol resultType)
            : base(resultType)
        {
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        }

        public IRListNode<IROutputColumnNode> Columns { get; }

        public override int ChildCount => 1;
        public override IRNode GetChild(int index) =>
            index switch
            {
                0 => Columns,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };


        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitProjectOperator(this, context);
        }

        public override string ToString()
        {
            return $"ProjectOperator: {ResultType.Display}";
        }
    }
}
