//
// Licensed under the MIT License.

using System;
using KustoLoco.Core.Evaluation;

namespace KustoLoco.Core.InternalRepresentation;

internal abstract class IRNode
{
    public virtual int ChildCount => 0;
    public virtual IRNode GetChild(int index) => throw new ArgumentOutOfRangeException();

    public abstract TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        where TResult : EvaluationResult;
}