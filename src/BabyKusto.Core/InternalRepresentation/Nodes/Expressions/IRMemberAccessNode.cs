// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRMemberAccessNode : IRExpressionNode
    {
        public IRMemberAccessNode(IRExpressionNode expression, string memberName, TypeSymbol resultType)
            : base(resultType, expression.ResultKind)
        {
            this.Expression = expression ?? throw new ArgumentNullException(nameof(expression));

            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException(nameof(memberName));
            }

            this.MemberName = memberName;
        }

        public IRExpressionNode Expression { get; }
        public string MemberName { get; }

        public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitMemberAccess(this, context);
        }

        public override string ToString()
        {
            return $"IRMemberAccess({MemberName}): {SchemaDisplay.GetText(ResultType)}";
        }
    }
}
