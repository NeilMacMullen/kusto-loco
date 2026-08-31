//
// Licensed under the MIT License.

using KustoLoco.Core.Evaluation;

namespace KustoLoco.Core.Evaluation.BuiltIns;

/// <summary>
///     A scalar function that also needs the per-query <see cref="EvaluationContext" /> (for example to reach a
///     host-registered provider via <c>context.Providers</c>). A function implements this in addition to being a normal
///     <see cref="IScalarFunctionImpl" />; when the evaluator sees it, it passes the context into the invocation. This
///     realises the standing TODO to thread <see cref="EvaluationContext" /> into scalar invocation without changing the
///     signature every other function depends on.
/// </summary>
internal interface IContextualScalarFunctionImpl
{
    ScalarResult InvokeScalar(ScalarResult[] arguments, EvaluationContext context);

    ColumnarResult InvokeColumnar(ColumnarResult[] arguments, EvaluationContext context);
}
