// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace BabyKusto.Core.InternalRepresentation
{
    internal abstract class IRListNode : IRNode
    {
        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitList(this, context);
        }

        public static IRListNode<T> From<T>(IReadOnlyList<T> items)
            where T : IRNode
        {
            return new IRListNode<T>(items);
        }
    }

    internal class IRListNode<T> : IRListNode
        where T : IRNode
    {
        private readonly IReadOnlyList<T> _children;

        public static IRListNode<T> Empty = new IRListNode<T>(new List<T>());

        public IRListNode(IReadOnlyList<T> children)
        {
            this._children = children ?? throw new ArgumentNullException(nameof(children));
        }

        public override int ChildCount => _children.Count;
        public override T GetChild(int index) => _children[index];

        public override string ToString()
        {
            return $"{PurgePrefixSuffix(typeof(T).Name)}[]";

            static string PurgePrefixSuffix(string name)
            {
                const string Prefix = "IR";
                if (name.StartsWith(Prefix))
                {
                    name = name.Substring(Prefix.Length);
                }

                const string Suffix = "Node";
                if (name.EndsWith(Suffix))
                {
                    name = name.Substring(0, name.Length - Suffix.Length);
                }

                return name;
            }
        }
    }
}
