// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using Kusto.Language.Symbols;
using Kusto.Language.Utils;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Evaluation.BuiltIns;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Other;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitBuiltInScalarFunctionCall(IRBuiltInScalarFunctionCallNode node,
        EvaluationContext context)
    {
        var hints = node.OverloadInfo.EvaluationHints;
        var isSelfGeneratingData =
            hints.HasFlag(EvaluationHints.ForceColumnarEvaluation);

        var resultKind = node.ResultKind;
        if (isSelfGeneratingData)
            resultKind = EvaluatedExpressionKind.Columnar;

        var impl = node.GetOrSetCache(() => BuiltInsHelper.GetScalarImplementation(node.OverloadInfo.ScalarImpl,
            resultKind, node.ResultType, hints));

        var arguments = new EvaluationResult[node.Arguments.ChildCount];
        for (var i = 0; i < node.Arguments.ChildCount; i++)
        {
            var argVal = node.Arguments.GetChild(i).Accept(this, context);
            Debug.Assert(argVal != null);
            arguments[i] = argVal;
        }

        if (isSelfGeneratingData)
        {
            //if the function "self generates" data we need to insert a 
            //'dummy' column that defines the length so that the 
            //function knows how many rows to generate for if that's 
            //not apparent from the other arguments.
            //TODO - really we should change the signature of the Invoke
            //method to pass in EvaluationContext - this is a bit of a hack
            var rowCount = context.Chunk.RowCount;
            var dummy =
                new ColumnarResult(
                    ColumnHelpers.CreateFromScalar(0, ScalarTypes.Int, rowCount)
                );
            arguments = new[] { dummy }.Concat(arguments).ToArray();
        }

        return impl(arguments);
    }

    public override EvaluationResult VisitBuiltInWindowFunctionCall(IRBuiltInWindowFunctionCallNode node,
        EvaluationContext context)
    {
        var impl = node.GetOrSetCache(() => BuiltInsHelper.GetWindowImplementation(
            node.OverloadInfo.Impl,
            node.ResultKind, node.ResultType));

        var arguments = new EvaluationResult[node.Arguments.ChildCount];
        for (var i = 0; i < node.Arguments.ChildCount; i++)
        {
            var argVal = node.Arguments.GetChild(i).Accept(this, context);
            Debug.Assert(argVal != null);
            arguments[i] = argVal;
        }

        return impl(arguments);
    }

    public override EvaluationResult VisitAggregateCallNode(IRAggregateCallNode node, EvaluationContext context)
    {
        Debug.Assert(context.Chunk != TableChunk.Empty);
        var impl = node.GetOrSetCache(() => node.OverloadInfo.AggregateImpl);

        var rawArguments = new EvaluationResult[node.Arguments.ChildCount];
        var hasScalar = false;
        for (var i = 0; i < node.Arguments.ChildCount; i++)
        {
            var argResult = node.Arguments.GetChild(i).Accept(this, context);
            Debug.Assert(argResult != null);
            rawArguments[i] = argResult;
            hasScalar = hasScalar || argResult.IsScalar;
        }
        var arguments = BuiltInsHelper.CreateResultArray(rawArguments);

        if (node.ResultType is TupleSymbol tuple)
        {
            foreach (var sym in tuple.Columns)
            {
                var index = context.Chunk.Table.Type.Columns.IndexOf(sym);
                var col = context.Chunk.Columns[index];
                var res = new ColumnarResult(col);
                arguments = arguments.Append(res).ToArray();
            }
        }

        return impl.Invoke(context.Chunk, arguments);
    }

    public override EvaluationResult VisitUserFunctionCall(IRUserFunctionCallNode node, EvaluationContext context)
    {
        var lookup = context.Scope.Lookup(node.Signature.Symbol.Name);
        if (lookup?.Symbol is not FunctionSymbol functionSymbol)
            throw new InvalidOperationException($"Function {node.Signature.Symbol.Name} not found.");

        // TODO: Signature matching, ideally from Kusto library
        var signature =
            functionSymbol.Signatures.FirstOrDefault(sig => sig.Parameters.Count == node.Arguments.ChildCount);
        if (signature == null)
            throw new InvalidOperationException(
                $"No matching signature with {node.Arguments.ChildCount} arguments for function {functionSymbol.Name}.");

        var functionCallScope = new LocalScope(context.Scope);
        for (var i = 0; i < node.Arguments.ChildCount; i++)
        {
            if (signature.Parameters[i].DeclaredTypes.Count != 1)
                throw new NotSupportedException(
                    $"Not sure how to deal with parameters having >1 DeclaredTypes (found {signature.Parameters[i].DeclaredTypes.Count} for function {functionSymbol.Name}.");

            var argValue = node.Arguments.GetChild(i).Accept(this, context);
            Debug.Assert(argValue != null);
            functionCallScope.AddSymbol(node.ParamSymbols[i], argValue);
        }

        return node.ExpandedBody.Accept(this, new EvaluationContext(functionCallScope));
    }

    public override EvaluationResult VisitFunctionBody(IRFunctionBodyNode node, EvaluationContext context)
    {
        var statements = node.Statements;
        for (var i = 0; i < statements.ChildCount; i++)
        {
            var statement = statements.GetChild(i);
            statement.Accept(this, context);
        }

        return node.Expression.Accept(this, context);
    }
}
