// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using BabyKusto.Core.Evaluation.BuiltIns;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult? VisitBuiltInFunctionCall(IRBuiltInFunctionCallNode node, EvaluationContext context)
        {
            var impl = node.GetOrSetCache(
                () =>
                {
                    var argumentExpressions = new IRExpressionNode[node.Arguments.ChildCount];
                    for (int i = 0; i < node.Arguments.ChildCount; i++)
                    {
                        argumentExpressions[i] = node.Arguments.GetChild(i);
                    }

                    var impl = node.OverloadInfo.ScalarImpl;
                    var resultKind = node.ResultKind;
                    return BuiltInsHelper.GetImplementation(argumentExpressions, impl, resultKind);
                });

            var arguments = new EvaluationResult[node.Arguments.ChildCount];
            for (int i = 0; i < node.Arguments.ChildCount; i++)
            {
                var argVal = node.Arguments.GetChild(i).Accept(this, context);
                Debug.Assert(argVal != null);
                arguments[i] = argVal;
            }

            return impl(arguments);
        }

        public override EvaluationResult? VisitAggregateCallNode(IRAggregateCallNode node, EvaluationContext context)
        {
            Debug.Assert(context.Chunk != null);
            var impl = node.GetOrSetCache(
                () =>
                {
                    var argumentExpressions = new IRExpressionNode[node.Arguments.ChildCount];
                    for (int i = 0; i < node.Arguments.ChildCount; i++)
                    {
                        argumentExpressions[i] = node.Arguments.GetChild(i);
                    }

                    return node.OverloadInfo.AggregateImpl;
                });

            var arguments = new ColumnarResult[node.Arguments.ChildCount];
            for (int i = 0; i < node.Arguments.ChildCount; i++)
            {
                var argResult = node.Arguments.GetChild(i).Accept(this, context);
                Debug.Assert(argResult != null);
                arguments[i] = (ColumnarResult)argResult;
            }

            return impl.Invoke(context.Chunk, arguments);
        }

        public override EvaluationResult? VisitUserFunctionCall(IRUserFunctionCallNode node, EvaluationContext context)
        {
            var lookup = context.Scope.Lookup(node.Signature.Symbol.Name);
            var functionSymbol = lookup?.Symbol as FunctionSymbol;
            if (functionSymbol == null)
            {
                throw new InvalidOperationException($"Function {node.Signature.Symbol.Name} not found.");
            }

            // TODO: Signature matching, ideally from Kusto library
            var signature = functionSymbol.Signatures.FirstOrDefault(sig => sig.Parameters.Count == node.Arguments.ChildCount);
            if (signature == null)
            {
                throw new InvalidOperationException($"No matching signature with {node.Arguments.ChildCount} arguments for function {functionSymbol.Name}.");
            }

            var functionCallScope = new LocalScope(context.Scope);
            for (int i = 0; i < node.Arguments.ChildCount; i++)
            {
                if (signature.Parameters[i].DeclaredTypes.Count != 1)
                {
                    throw new NotSupportedException($"Not sure how to deal with parameters having >1 DeclaredTypes (found {signature.Parameters[i].DeclaredTypes.Count} for function {functionSymbol.Name}.");
                }

                var argValue = node.Arguments.GetChild(i).Accept(this, context);
                Debug.Assert(argValue != null);
                functionCallScope.AddSymbol(node.ParamSymbols[i], argValue);
            }

            return node.ExpandedBody.Accept(this, new EvaluationContext(functionCallScope));
        }

        public override EvaluationResult? VisitFunctionBody(IRFunctionBodyNode node, EvaluationContext context)
        {
            var statements = node.Statements;
            for (int i = 0; i < statements.ChildCount; i++)
            {
                var statement = statements.GetChild(i);
                statement.Accept(this, context);
            }

            return node.Expression.Accept(this, context);
        }
    }
}
