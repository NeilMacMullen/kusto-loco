// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace BabyKusto.Core.InternalRepresentation
{
    internal abstract class IRNode
    {
        public virtual int ChildCount => 0;
        public virtual IRNode GetChild(int index) => throw new ArgumentOutOfRangeException();

        public abstract TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult: class;
    }
}
