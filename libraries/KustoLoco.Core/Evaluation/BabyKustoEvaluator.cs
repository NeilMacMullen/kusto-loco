//
// Licensed under the MIT License.

using KustoLoco.Core.InternalRepresentation;

namespace KustoLoco.Core.Evaluation;

internal static class BabyKustoEvaluator
{
    internal static EvaluationResult Evaluate(IRNode root, LocalScope scope)
    {
        var evaluator = new TreeEvaluator();
        return root.Accept(evaluator, new EvaluationContext(scope));
    }
}